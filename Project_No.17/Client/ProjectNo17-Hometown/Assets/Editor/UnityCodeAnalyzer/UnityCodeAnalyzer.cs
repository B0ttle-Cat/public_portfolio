using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using UnityEditor;

using UnityEngine;

public class UnityCodeAnalyzer : EditorWindow
{
	enum ViewMode { Tree, List }
	ViewMode viewMode = ViewMode.Tree;
	bool includeExternalTypes = true;

	string folderPath = "";
	Vector2 scrollPos;
	FolderNode rootFolder;
	HashSet<string> internalTypes = new();

	[MenuItem("Tools/Roslyn 클래스 코드 분석기")]
	public static void ShowWindow()
	{
		GetWindow<UnityCodeAnalyzer>("클래스 코드 분석기");
	}

	void OnGUI()
	{
		GUILayout.Label("Roslyn 클래스 코드 분석기", EditorStyles.boldLabel);

		GUILayout.Space(10);

		GUILayout.Label("분석할 폴더 선택");
		GUILayout.BeginHorizontal();
		folderPath = GUILayout.TextField(folderPath);
		if(GUILayout.Button("폴더 선택", GUILayout.Width(100)))
		{
			folderPath = EditorUtility.OpenFolderPanel("분석할 폴더 선택", Application.dataPath, "");
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(5);
		viewMode = (ViewMode)EditorGUILayout.EnumPopup("보기 방식", viewMode);
		includeExternalTypes = EditorGUILayout.Toggle("외부 타입 포함", includeExternalTypes);

		if(GUILayout.Button("분석 시작", GUILayout.Height(40)))
		{
			AnalyzeFolder();
		}

		GUILayout.Space(10);

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		if(rootFolder != null)
		{
			if(viewMode == ViewMode.Tree) DrawFolderNode(rootFolder);
			else DrawFileList(rootFolder);
		}
		EditorGUILayout.EndScrollView();
	}

	void AnalyzeFolder()
	{
		if(string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
			return;

		internalTypes.Clear();
		rootFolder = new FolderNode(new DirectoryInfo(folderPath));
		var csFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

		foreach(var file in csFiles)
		{
			var code = File.ReadAllText(file);
			var tree = CSharpSyntaxTree.ParseText(code);
			var compilation = CSharpCompilation.Create("Temp")
				.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
				.AddSyntaxTrees(tree);
			var model = compilation.GetSemanticModel(tree);
			var analyzer = new TypeReferenceCollector(model);
			analyzer.Visit(tree.GetRoot());

			foreach(var type in analyzer.DefinedTypes)
				internalTypes.Add(type);

			var relativePath = Path.GetRelativePath(folderPath, file);
			rootFolder.AddFile(relativePath, analyzer);
		}
	}

	bool IsInternal(string type)
	{
		return internalTypes.Contains(type) || includeExternalTypes;
	}

	void DrawFolderNode(FolderNode node)
	{
		node.IsExpanded = EditorGUILayout.Foldout(node.IsExpanded, node.Name);
		if(node.IsExpanded)
		{
			EditorGUI.indentLevel++;
			foreach(var childFolder in node.SubFolders.Values)
				DrawFolderNode(childFolder);

			foreach(var file in node.Files)
			{
				file.IsExpanded = EditorGUILayout.Foldout(file.IsExpanded, file.Name);
				if(file.IsExpanded)
				{
					EditorGUI.indentLevel++;
					GUILayout.Label(file.Analyzer.GetAnalysisSummary(IsInternal), EditorStyles.wordWrappedLabel);
					EditorGUI.indentLevel--;
				}
			}
			EditorGUI.indentLevel--;
		}
	}

	void DrawFileList(FolderNode node)
	{
		foreach(var file in node.Files)
		{
			file.IsExpanded = EditorGUILayout.Foldout(file.IsExpanded, file.FullPath);
			if(file.IsExpanded)
			{
				EditorGUI.indentLevel++;
				GUILayout.Label(file.Analyzer.GetAnalysisSummary(IsInternal), EditorStyles.wordWrappedLabel);
				EditorGUI.indentLevel--;
			}
		}

		foreach(var childFolder in node.SubFolders.Values)
			DrawFileList(childFolder);
	}

	class FolderNode
	{
		public string Name;
		public Dictionary<string, FolderNode> SubFolders = new();
		public List<FileNode> Files = new();
		public bool IsExpanded;

		public FolderNode(DirectoryInfo dir)
		{
			Name = dir.Name;
		}

		public void AddFile(string relativePath, TypeReferenceCollector analyzer)
		{
			var parts = relativePath.Split(Path.DirectorySeparatorChar);
			if(parts.Length == 1)
				Files.Add(new FileNode(parts[0], relativePath, analyzer));
			else
			{
				if(!SubFolders.ContainsKey(parts[0]))
					SubFolders[parts[0]] = new FolderNode(new DirectoryInfo(parts[0]));

				SubFolders[parts[0]].AddFile(string.Join(Path.DirectorySeparatorChar, parts.Skip(1)), analyzer);
			}
		}
	}

	class FileNode
	{
		public string Name;
		public string FullPath;
		public TypeReferenceCollector Analyzer;
		public bool IsExpanded;

		public FileNode(string name, string fullPath, TypeReferenceCollector analyzer)
		{
			Name = name;
			FullPath = fullPath;
			Analyzer = analyzer;
		}
	}

	class TypeReferenceCollector : CSharpSyntaxWalker
	{
		private readonly SemanticModel _semanticModel;

		public class TypeReference
		{
			public string Category;           // Field, Property, Parameter, Return, Local, ObjectCreation 등
			public string MemberName;         // 변수명, 메서드명 등
			public ITypeSymbol ActualType;    // 전체 타입 (예: List<MyType>)
			public List<ITypeSymbol> ReferencedTypes; // 추출된 하위 타입들 (예: MyType)
		}

		public List<TypeReference> References = new();

		public TypeReferenceCollector(SemanticModel model)
		{
			_semanticModel = model;
		}

		public HashSet<string> DefinedTypes = new();
		public override void VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			DefinedTypes.Add(node.Identifier.Text);
			base.VisitClassDeclaration(node);
		}

		public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
		{
			var typeSymbol = _semanticModel.GetTypeInfo(node.Declaration.Type).Type;

			foreach(var variable in node.Declaration.Variables)
			{
				References.Add(new TypeReference {
					Category = "Field",
					MemberName = variable.Identifier.Text,
					ActualType = typeSymbol,
					ReferencedTypes = ExtractReferencedTypes(typeSymbol)
				});
			}

			base.VisitFieldDeclaration(node);
		}

		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
		{
			var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type;

			References.Add(new TypeReference {
				Category = "Property",
				MemberName = node.Identifier.Text,
				ActualType = typeSymbol,
				ReferencedTypes = ExtractReferencedTypes(typeSymbol)
			});

			base.VisitPropertyDeclaration(node);
		}

		public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			var methodSymbol = _semanticModel.GetDeclaredSymbol(node);

			// 반환 타입
			References.Add(new TypeReference {
				Category = "MethodReturn",
				MemberName = methodSymbol?.Name ?? node.Identifier.Text,
				ActualType = methodSymbol?.ReturnType,
				ReferencedTypes = ExtractReferencedTypes(methodSymbol?.ReturnType)
			});

			// 매개변수들
			foreach(var param in node.ParameterList.Parameters)
			{
				var paramSymbol = _semanticModel.GetDeclaredSymbol(param);
				References.Add(new TypeReference {
					Category = "Parameter",
					MemberName = paramSymbol?.Name ?? param.Identifier.Text,
					ActualType = paramSymbol?.Type,
					ReferencedTypes = ExtractReferencedTypes(paramSymbol?.Type)
				});
			}

			base.VisitMethodDeclaration(node);
		}

		public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
		{
			var typeSymbol = _semanticModel.GetTypeInfo(node.Declaration.Type).Type;

			foreach(var variable in node.Declaration.Variables)
			{
				References.Add(new TypeReference {
					Category = "LocalVariable",
					MemberName = variable.Identifier.Text,
					ActualType = typeSymbol,
					ReferencedTypes = ExtractReferencedTypes(typeSymbol)
				});
			}

			base.VisitLocalDeclarationStatement(node);
		}

		public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
		{
			var typeSymbol = _semanticModel.GetTypeInfo(node).Type;

			References.Add(new TypeReference {
				Category = "ObjectCreation",
				MemberName = node.Type.ToString(),
				ActualType = typeSymbol,
				ReferencedTypes = ExtractReferencedTypes(typeSymbol)
			});

			base.VisitObjectCreationExpression(node);
		}

		// 핵심: List<T>, T[], Dictionary<K,V> 등에서 실제 하위 타입만 추출
		private List<ITypeSymbol> ExtractReferencedTypes(ITypeSymbol symbol)
		{
			var list = new List<ITypeSymbol>();
			if(symbol == null) return list;

			if(symbol is IArrayTypeSymbol arrayType)
			{
				list.Add(arrayType.ElementType);
			}
			else if(symbol is INamedTypeSymbol named && named.IsGenericType)
			{
				list.AddRange(named.TypeArguments);
			}
			else
			{
				list.Add(symbol);
			}

			return list;
		}

		public string GetAnalysisSummary(System.Func<string, bool> filter)
		{
			return "";
		}
	}
}