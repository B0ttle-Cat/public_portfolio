using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

using UnityEditor;

using UnityEngine;

public class UnityCodeAnalyzer : OdinEditorWindow
{
	enum ViewMode
	{
		[LabelText("간략히 보기")] SimpleView,
		[LabelText("자세히 보기")] DetailView,
	}
	[Flags]
	enum TypeFilterMode
	{
		[LabelText("보지 않기")] None = 0,
		[LabelText("동일 Namespace 보기")]Internal = 1<<0,
		[LabelText("다른 Namespace 보기")]External = 1<<1,
		[LabelText("기본(System+Unity) 타입 보기")]BuiltIn = 1<<2,
		[LabelText("global namespace 보기")]Global = 1<<3,
		[LabelText("사용자 정의 타입 보기")] UserType = Internal | External,
		[LabelText("모두 보기")] All = Internal | External | BuiltIn | Global, 
	}

	// TItle
	[TitleGroup("검색 옵션",order: -99)]
	[SerializeField, FolderPath, LabelText("루트 폴더 선택")]
	private string folderPath = "Assets/Main";
	[TitleGroup("검색 옵션")]
	[HorizontalGroup("검색 옵션/AnalyzerOption"), SerializeField, LabelText("뷰 모드 선택")]
	private ViewMode viewMode = ViewMode.SimpleView;
	// 외부에 존재하는 타입을 포함?
	[TitleGroup("검색 옵션")]
	[HorizontalGroup("검색 옵션/AnalyzerOption"), SerializeField, LabelText("참조 표시 타입 선택")]
	private TypeFilterMode typeFilterMode = TypeFilterMode.All;

	// Result
	// 내부에 존재하는 타입들
	private HashSet<string> internalTypes = new();

	private List<FileNode> AllFileList;
	[TitleGroup("분석 결과")]
	[CustomValueDrawer(nameof(HasResultCustomValueDrawer))]
	public bool HasResult;
	private List<ClassNode> ClassList;

	static Vector2 scrollPosition;

	private class FileNode
	{
		public string Name;
		public string FullPath;
		public TypeReferenceCollector Analyzer;
		public FileNode(string name, string fullPath, TypeReferenceCollector analyzer)
		{
			Name = name;
			FullPath = fullPath;
			Analyzer = analyzer;
		}

		internal void Clear()
		{
			if(Analyzer != null)
			{
				Analyzer.Clear();
				Analyzer = null;
			}
		}
	}
	[Serializable, InlineProperty, HideLabel, HideReferenceObjectPicker]
	private class ClassNode
	{
		public string TypeName;
		public List<TypeReferenceString> DetailReferencesTypes;
		public List<TypeReferenceString> SimpleReferencesTypes;
		[HideInInspector]
		public bool isShowFoldout = false;

		public ClassNode(string fullName, List<TypeReferenceSymbol> references)
		{
			TypeName = fullName;
			string NamespaceText = fullName.Substring(0, fullName.LastIndexOf('.'));
			DetailReferencesTypes = references.Select(r =>
				new TypeReferenceString() {
					Category = r.Category,
					MemberName = r.MemberName,
					TypeNameList = new List<string> { GetFullTypeName(r.ActualType) },
				}
			).ToList();
			SimpleReferencesTypes = new List<TypeReferenceString>();

			int length = DetailReferencesTypes.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var d = DetailReferencesTypes[i];
				int findIndex = SimpleReferencesTypes.FindIndex(s => s.Category == d.Category);
				if(findIndex < 0)
				{
					SimpleReferencesTypes.Add(new TypeReferenceString() {
						Category = d.Category,
						MemberName = "",
						TypeNameList = d.TypeNameList,
					});
				}
				else
				{
					var s = SimpleReferencesTypes[findIndex];
					List<string> simpleList = s.TypeNameList;
					simpleList.AddRange(d.TypeNameList);
					s.TypeNameList = simpleList.ToHashSet().ToList();
					SimpleReferencesTypes[findIndex] = s;
				}
			}
		}
		bool IsSameNamespace(string namespaceText, ITypeSymbol b)
		{
			var nsB = b.ContainingNamespace?.ToDisplayString();
			return namespaceText == nsB;
		}
		string GetFullTypeName(ITypeSymbol typeSymbol)
		{
			if(typeSymbol == null) return "";
			if(typeSymbol.ContainingNamespace == null) return typeSymbol.ToDisplayString();
			return typeSymbol.ContainingNamespace.ToDisplayString() +"."+ typeSymbol.ToDisplayString();
		}

		private HashSet<ITypeSymbol> ExtractReferencedTypes(ITypeSymbol symbol)
		{
			var list = new HashSet<ITypeSymbol>();
			if(symbol == null) return list;

			_ExtractReferencedTypes(symbol);

			return list;

			void _ExtractReferencedTypes(ITypeSymbol inSymbol)
			{
				if(inSymbol is IArrayTypeSymbol arrayType)
				{
					_ExtractReferencedTypes(arrayType.ElementType);
				}
				else if(inSymbol is INamedTypeSymbol named && named.IsGenericType)
				{
					foreach(var typeArgument in named.TypeArguments)
					{
						_ExtractReferencedTypes(typeArgument);
					}
				}
				else
				{
					list.Add(inSymbol);
				}
			}
		}
 
		internal void Open()
		{
			isShowFoldout = true;
		}

		internal void Close()
		{
			isShowFoldout = false;
		}

		internal void Clear()
		{
			if(DetailReferencesTypes != null)
			{
				foreach(var r in DetailReferencesTypes)
				{
					r.Dispose();
				}
				DetailReferencesTypes = null;
			}
		}
	}
	static Rect position;

	private bool HasResultCustomValueDrawer(bool hasResult, GUIContent label)
	{
		if(ClassList == null || ClassList.Count == 0)
		{
			return false;
		}
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		int length = ClassList.Count;
		for(int i = 0 ; i < length ; i++)
		{
			ClassNode classNode = ClassList[i];
			if(classNode == null) continue;
			if(string.IsNullOrWhiteSpace(classNode.TypeName)) continue;
			try
			{
				classNode.isShowFoldout = SirenixEditorGUI.Foldout(classNode.isShowFoldout, classNode.TypeName, EditorStyles.foldout);
				if(classNode.isShowFoldout)
				{
				
					GUIStyle style = new GUIStyle(EditorStyles.label)
					{
						richText = true,
						wordWrap = true,
					};

					string thisNamespace = classNode.TypeName.Substring(0, classNode.TypeName.LastIndexOf('.'));

					if(viewMode == ViewMode.SimpleView)
					{
						if(classNode.SimpleReferencesTypes != null)
						{
							var stringList = classNode.SimpleReferencesTypes.Select(s => s.ToString(thisNamespace, this)).Where(w=>!string.IsNullOrWhiteSpace(w)).ToList();
							string joinedText = string.Join("\n", stringList);

							var content = new GUIContent(joinedText);
							float width = EditorGUIUtility.currentViewWidth;// - 40f; // 좌우 마진 고려
							float height = style.CalcHeight(content, width) + 10f;
							var rect = EditorGUILayout.GetControlRect(false, height);

							EditorGUI.indentLevel++;
							EditorGUI.SelectableLabel(rect, joinedText, style);
							EditorGUI.indentLevel--;
						}
					}
					else
					{
						if(classNode.DetailReferencesTypes != null)
						{
							var stringList = classNode.DetailReferencesTypes.Select(s => s.ToString(thisNamespace, this)).Where(w=>!string.IsNullOrWhiteSpace(w)).ToList();
							string joinedText = string.Join("\n", stringList);

							var content = new GUIContent(joinedText);
							float width = EditorGUIUtility.currentViewWidth - 40f; // 좌우 마진 고려
							float height = style.CalcHeight(content, width);
							var rect = EditorGUILayout.GetControlRect(false, height);

							EditorGUI.indentLevel++;
							EditorGUI.SelectableLabel(rect, joinedText, style);
							EditorGUI.indentLevel--;
						}
					}
				}
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		EditorGUILayout.EndScrollView();

		return true;
	}

	[MenuItem("Tools/Roslyn 클래스 코드 분석기")]
	public static void OpenWindow()
	{
		GetWindow<UnityCodeAnalyzer>("클래스 코드 분석기");
	}
	private string enableAnalyzerString { get; set; } = "";
	private bool IsEnableAnalyzerFlag => walkTask != null;
	private string LabelButtonName {
		get {
			if(IsEnableAnalyzerFlag)
			{
				return $"분석 중... {enableAnalyzerString}";
			}
			else
			{
				return "분석 시작";
			}
		}
	}
	private Task walkTask = null;
	private CancellationTokenSource token = null;
	[ButtonGroup("CodeAnalyzerButton"), PropertyOrder(-98)]
	[Button("@LabelButtonName"), DisableIf(@"IsEnableAnalyzerFlag")]
	private async void OnStartCodeAnalyzer()
	{
		token = new CancellationTokenSource();
		walkTask = OnCodeAnalyzerAsync(token.Token);
		await walkTask;
		token = null;
		walkTask = null;
	}
	[ButtonGroup("CodeAnalyzerButton")]
	[Button("분석 중단"), EnableIf(@"IsEnableAnalyzerFlag")]
	private void OnStopCodeAnalyzer()
	{
		if(token != null)
		{
			token.Cancel();
			token = null;
			walkTask = null;
		}
	}
	[ButtonGroup("CodeAnalyzerButton")]
	[Button("결과 지우기"), DisableIf("@HasResult")]
	private void OnClearCodeAnalyzer()
	{
		if(AllFileList != null)
		{
			AllFileList.ForEach(f => f.Clear());
			AllFileList.Clear();
			AllFileList = null;
		}
		if(ClassList != null)
		{
			ClassList.ForEach(f => f.Clear());
			ClassList.Clear();
			ClassList = null;
		}
	}

	[ButtonGroup("OpenAndClose"), PropertyOrder(-90)]
	[Button("모두 펼치기"), EnableIf("@HasResult")]
	private void AllOpen()
	{
		if(ClassList == null) return;
		ClassList.ForEach(c => c.Open());
	}
	[ButtonGroup("OpenAndClose"), PropertyOrder(-90)]
	[Button("모두 접기"), EnableIf("@HasResult")]
	private void AllClose()
	{
		if(ClassList == null) return;
		ClassList.ForEach(c => c.Close());
	}

	private async Task OnCodeAnalyzerAsync(CancellationToken token)
	{
		await AnalyzeFolder(token);
		await AnalyzeReferences(token);
		enableAnalyzerString = null;
	}

	async Task AnalyzeFolder(CancellationToken token)
	{
		if(token.IsCancellationRequested) return;
		if(string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
			return;

		enableAnalyzerString = $"검사할 .cs 파일 목록 구성";
		await Task.Delay(100);

		var csFiles = Directory
			.EnumerateFiles(folderPath, "*.cs", SearchOption.AllDirectories)
			.Where(filePath => !IsHiddenPath(filePath, folderPath))
			.ToArray();

		// 숨김 파일 또는 숨김 폴더 여부를 검사하는 메서드
		bool IsHiddenPath(string fullPath, string rootPath)
		{
			// root 경로를 제거하고 상대경로로 변환
			string relativePath = fullPath.Substring(rootPath.Length).TrimStart(Path.DirectorySeparatorChar);
			// 경로의 각 파트(폴더, 파일명)을 검사하여 숨김인지 확인
			return relativePath.Split(Path.DirectorySeparatorChar)
				.Any(part => part.StartsWith(".") || part.StartsWith("~"));
		}

		internalTypes.Clear();
		AllFileList = new List<FileNode>();
		ConcurrentDictionary<string, TypeReferenceCollector> fileAnalyzerSet = new ConcurrentDictionary<string, TypeReferenceCollector>();

		//var assemblyList = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
		var referencesList = AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(asm => !asm.IsDynamic && !string.IsNullOrEmpty(asm.Location))
			.Select(asm => MetadataReference.CreateFromFile(asm.Location));

		int allCount = csFiles.Length;
		object countLock = new object();
		int readCount = 0;
		enableAnalyzerString = $"코드 읽기 시작 ({readCount}/{allCount})";
		await Task.Delay(100);
#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
		Task.Run(() => {
			Parallel.ForEach(csFiles, async (string csFilePath, ParallelLoopState state) => {
				try
				{
					Task<string> readTask = File.ReadAllTextAsync(csFilePath, token);
					await readTask;
					if(token.IsCancellationRequested)
					{
						state.Break();
						return;
					}
					string code = readTask.Result;

					SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
					SyntaxNode root = tree.GetRoot();
					CSharpCompilation compilation = CSharpCompilation.Create("Temp")
						.AddReferences(referencesList)
						.AddSyntaxTrees(tree);
					SemanticModel model = compilation.GetSemanticModel(tree);
					TypeReferenceCollector analyzer = new TypeReferenceCollector(model);
					analyzer.Visit(root);

					string relativePath = Path.GetRelativePath(folderPath, csFilePath);
					fileAnalyzerSet.TryAdd(relativePath, analyzer);

					lock(countLock)
					{
						++readCount;
					}
				}
				catch(Exception e)
				{
					Debug.LogError(e);
					state.Break();
				}
			});
		});
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
		while(allCount != readCount)
		{
			enableAnalyzerString = $"코드 읽기 진행 중... ({readCount}/{allCount})";
			if(token.IsCancellationRequested)
			{
				return;
			}
			await Task.Delay(100);
		}
		enableAnalyzerString = $"코드 읽기 완료";
		await Task.Delay(500);

		//var sortedAnalyzerSet = fileAnalyzerSet.OrderBy(item => item.Key);
		foreach(var item in fileAnalyzerSet)
		{
			string relativePath =  item.Key;
			TypeReferenceCollector analyzer = item.Value;
			foreach(var type in analyzer.DefinedTypes)
			{
				string fullName = type == null ? ""
				: type.ContainingNamespace == null
				? $"{type.ToDisplayString()}"
				: $"{type.ContainingNamespace.ToDisplayString()}.{type.ToDisplayString()}";
				internalTypes.Add(fullName);
			}
			AllFileList.Add(new FileNode(relativePath.Split(Path.DirectorySeparatorChar)[^1], relativePath, analyzer));
		}
		enableAnalyzerString = null;
	}

	async Task AnalyzeReferences(CancellationToken token)
	{
		if(token.IsCancellationRequested) return;
		if(AllFileList == null || AllFileList.Count == 0) return;
		if(internalTypes == null || internalTypes.Count == 0) return;

		enableAnalyzerString = $"참조 목록 작성";
		await Task.Delay(100);

		ClassList = new List<ClassNode>();
		IEnumerable<TypeReferenceSymbol> allReferenceList = AllFileList.SelectMany(f=>f.Analyzer.References);
		Dictionary<string, ConcurrentBag<TypeReferenceSymbol>> referenceMap = new Dictionary<string, ConcurrentBag<TypeReferenceSymbol>>(internalTypes.Count);
		foreach(var typeFullName in internalTypes)
		{
			referenceMap[typeFullName] = new ConcurrentBag<TypeReferenceSymbol>();
		}

		int allCount = allReferenceList.Count();
		object countLock = new object();
		int readCount = 0;
		enableAnalyzerString = $"참조 분류 시작 ({readCount}/{allCount})";
#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
		Task.Run(() => {
			Parallel.ForEach(allReferenceList, async (TypeReferenceSymbol reference, ParallelLoopState state) => {
				try
				{
					if(token.IsCancellationRequested)
					{
						state.Break();
						return;
					}
					var typeSymbol = reference.NamedTypeSymbol;
					string FullName = typeSymbol == null ? ""
						: typeSymbol.ContainingNamespace == null
						? $"{typeSymbol.ToDisplayString()}"
						: $"{typeSymbol.ContainingNamespace.ToDisplayString()}.{typeSymbol.ToDisplayString()}";

					var bag = referenceMap[FullName];
					bag.Add(reference);

					lock(countLock)
					{
						++readCount;
					}
				}
				catch(Exception e)
				{
					Debug.LogError(e);
					state.Break();
				}
			});
		});
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.

		while(allCount != readCount)
		{
			enableAnalyzerString = $"참조 분류 진행 중... ({readCount}/{allCount})";
			if(token.IsCancellationRequested)
			{
				return;
			}
			await Task.Delay(100);
		}
		enableAnalyzerString = $"참조 분류 완료";
		await Task.Delay(500);

		//referenceMap.
		var sortedReferenceMap = referenceMap
			.OrderBy(item => item.Key);
		foreach(var item in sortedReferenceMap)
		{
			var namedType = item.Key;
			var refList = item.Value
				.OrderBy(o=>o.MemberName)
				.OrderBy(o=>o.Category)
				.ToList();
			ClassList.Add(new ClassNode(namedType, refList));
		}
		enableAnalyzerString = null;
	}

	public enum CategoryType
	{
		Class = 0, Interface, Struct,
		BaseClass, BaseInterface,
		Field, Property, Parameter, Return, Local, ObjectCreation,
	}
	public enum SubCategoryType
	{
		None, IsArray, IsGeneric
	}
	public enum IncludeType
	{
		Internal, External, BuiltIn
	}
	[Serializable]
	public struct TypeReferenceString : IDisposable
	{
		public CategoryType Category;      // Field, Property, Parameter, Return, Local, ObjectCreation 등
		public SubCategoryType SubCategory;      // Field, Property, Parameter, Return, Local, ObjectCreation 등

		public string MemberName;
		public List<string> TypeNameList;
		private string CategoryString()
		{
			return Category switch {
				CategoryType.Class => "<color=#5F5>[Class]",
				CategoryType.Interface => "<color=#5F5>[Interface]",
				CategoryType.Struct => "<color=#5F5>[Struct]",
				CategoryType.BaseClass => "<color=#AFA>[상속]",
				CategoryType.BaseInterface => "<color=#AFA>[구현]",
				CategoryType.Field => "<color=#AAF>[필드타입]",
				CategoryType.Property => "<color=#AAF>[프로퍼티]",
				CategoryType.Parameter => "<color=#FAF>[매개변수]",
				CategoryType.Return => "<color=#FAF>[반환타입]",
				CategoryType.Local => "<color=#5AF>[로컬선언]",
				CategoryType.ObjectCreation => "<color=#5AF>[생성타입]",
				_ => "",
			} + SubCategory switch {
				SubCategoryType.None => "",
				SubCategoryType.IsArray => "<color=#FFA>[Array]",
				SubCategoryType.IsGeneric => "<color=#AFF>[Generic]",
				_ => "",
			};
		}

		public void Dispose()
		{
			MemberName = null;
			TypeNameList = null;
		}

		public string ToString(string thisNamespace, UnityCodeAnalyzer thisView)
		{
			bool detail = thisView.viewMode == ViewMode.DetailView;
			var typeNameList  = TypeNameList.Select(s=>{

				if(thisView.typeFilterMode == TypeFilterMode.None)
				{
					return "";
				}
				string NamespaceText = s.Substring(0, s.LastIndexOf('.'));

				bool isInternal = NamespaceText.Equals(thisNamespace);
				bool isBuiltIn = s.StartsWith("System.") || s.StartsWith("Unity.") || s.StartsWith("UnityEngine.") ||  s.StartsWith("UnityEditor.");
				bool isGlobal = s.StartsWith("<global namespace>");
				if(isInternal && thisView.typeFilterMode.HasFlag(TypeFilterMode.Internal))
				{
					return $"<color=#AF5>{s}</color>";
				}
				if(!isInternal && !isBuiltIn && thisView.typeFilterMode.HasFlag(TypeFilterMode.External))
				{
					return $"<color=#FA5>{s}</color>";
				}
				if(!isInternal && isBuiltIn && thisView.typeFilterMode.HasFlag(TypeFilterMode.BuiltIn))
				{
					return $"<color=#fff>{s}</color>";
				}
				if(isGlobal && thisView.typeFilterMode.HasFlag(TypeFilterMode.Global))
				{
					return $"<color=#555>{s}</color>";
				}
				return "";
			}).Where(w=>!string.IsNullOrWhiteSpace(w)).ToArray();

			if(typeNameList.Count() > 0)
			{
				return detail
				? $"{CategoryString()}</color>     <color=#FF5>{(string.IsNullOrWhiteSpace(MemberName) ? "<unname>" : MemberName)}</color>     {typeNameList[0]}"
				: $"{CategoryString()}</color>\n\t{string.Join("\n\t", typeNameList)}";
			}
			else
			{
				return "";
			}
		}
	}
	public class TypeReferenceSymbol : IDisposable
	{
		public INamedTypeSymbol NamedTypeSymbol;

		public CategoryType Category;           // Field, Property, Parameter, Return, Local, ObjectCreation 등
		public SubCategoryType SubCategory;
		public string MemberName;         // 변수명, 메서드명 등
		public ITypeSymbol ActualType;    // 전체 타입 (예: List<MyType>)
		public void Dispose()
		{
			NamedTypeSymbol = null;
			ActualType = null;
			//	SubActualType = null;
		}
	}
	class TypeReferenceCollector : CSharpSyntaxWalker
	{
		private SemanticModel _semanticModel;
		INamedTypeSymbol _currentClassSymbol;

		public List<TypeReferenceSymbol> References = new();

		public TypeReferenceCollector(SemanticModel model)
		{
			_semanticModel = model;
		}

		public HashSet<INamedTypeSymbol> DefinedTypes = new();
		public override void VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			INamedTypeSymbol symbol = _semanticModel.GetDeclaredSymbol(node);
			if(symbol == null || symbol.TypeKind != TypeKind.Class) return;

			_currentClassSymbol = symbol;
			DefinedTypes.Add(_currentClassSymbol);
			References.AddRange(ExtractReferencedTypes(_currentClassSymbol).Select(s => new TypeReferenceSymbol {
				NamedTypeSymbol = _currentClassSymbol,
				Category = CategoryType.Class,
				MemberName = "",
				ActualType = s.Item1,
				SubCategory = s.Item2,
			}));

			var baseType = _currentClassSymbol.BaseType;
			var interfaces = _currentClassSymbol.Interfaces;
			if(baseType != null && baseType.Name != "Object" && baseType.Name != "object")
			{
				References.AddRange(ExtractReferencedTypes(baseType).Select(s => new TypeReferenceSymbol {
					NamedTypeSymbol = _currentClassSymbol,
					Category = CategoryType.BaseClass,
					MemberName = "",
					ActualType = s.Item1,
					SubCategory = s.Item2,
				}));
			}
			foreach(var @interface in interfaces)
			{
				References.AddRange(ExtractReferencedTypes(@interface).Select(s => new TypeReferenceSymbol {
					NamedTypeSymbol = _currentClassSymbol,
					Category = CategoryType.BaseInterface,
					MemberName = "",
					ActualType = s.Item1,
					SubCategory = s.Item2,
				}));
			}

			base.VisitClassDeclaration(node);
		}
		public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
		{
			INamedTypeSymbol symbol = _semanticModel.GetDeclaredSymbol(node);
			if(symbol == null || symbol.TypeKind != TypeKind.Interface) return;


			_currentClassSymbol = symbol;
			DefinedTypes.Add(_currentClassSymbol);
			References.AddRange(ExtractReferencedTypes(_currentClassSymbol).Select(s => new TypeReferenceSymbol {
				NamedTypeSymbol = _currentClassSymbol,
				Category = CategoryType.Interface,
				MemberName = "",
				ActualType = s.Item1,
				SubCategory = s.Item2,
			}));

			var interfaces = _currentClassSymbol.Interfaces;

			foreach(var @interface in interfaces)
			{
				References.AddRange(ExtractReferencedTypes(@interface).Select(s => new TypeReferenceSymbol {
					NamedTypeSymbol = _currentClassSymbol,
					Category = CategoryType.BaseInterface,
					MemberName = "",
					ActualType = s.Item1,
					SubCategory = s.Item2,
				}));
			}
			base.VisitInterfaceDeclaration(node);
		}
		public override void VisitStructDeclaration(StructDeclarationSyntax node)
		{
			INamedTypeSymbol symbol = _semanticModel.GetDeclaredSymbol(node);
			if(symbol == null || symbol.TypeKind != TypeKind.Struct) return;


			_currentClassSymbol = symbol;
			DefinedTypes.Add(_currentClassSymbol);
			References.AddRange(ExtractReferencedTypes(_currentClassSymbol).Select(s => new TypeReferenceSymbol {
				NamedTypeSymbol = _currentClassSymbol,
				Category = CategoryType.Struct,
				MemberName = "",
				ActualType = s.Item1,
				SubCategory = s.Item2,
			}));

			var interfaces = _currentClassSymbol.Interfaces;
			foreach(var @interface in interfaces)
			{
				References.AddRange(ExtractReferencedTypes(@interface).Select(s => new TypeReferenceSymbol {
					NamedTypeSymbol = _currentClassSymbol,
					Category = CategoryType.BaseInterface,
					MemberName = "",
					ActualType = s.Item1,
					SubCategory = s.Item2,
				}));
			}
			base.VisitStructDeclaration(node);
		}

		public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
		{
			var typeSymbol = _semanticModel.GetTypeInfo(node.Declaration.Type).Type;

			foreach(var variable in node.Declaration.Variables)
			{
				References.AddRange(ExtractReferencedTypes(typeSymbol).Select(s => new TypeReferenceSymbol {
					NamedTypeSymbol = _currentClassSymbol,
					Category = CategoryType.Field,
					MemberName = variable.Identifier.Text,
					ActualType = s.Item1,
					SubCategory = s.Item2,
				}));
			}

			base.VisitFieldDeclaration(node);
		}

		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
		{
			var typeSymbol = _semanticModel.GetTypeInfo(node.Type).Type;

			References.AddRange(ExtractReferencedTypes(typeSymbol).Select(s => new TypeReferenceSymbol {
				NamedTypeSymbol = _currentClassSymbol,
				Category = CategoryType.Property,
				MemberName = node.Identifier.Text,
				ActualType = s.Item1,
				SubCategory = s.Item2,
			}));
			base.VisitPropertyDeclaration(node);
		}

		public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			var methodSymbol = _semanticModel.GetDeclaredSymbol(node);

			// 반환 타입
			References.AddRange(ExtractReferencedTypes(methodSymbol?.ReturnType).Select(s => new TypeReferenceSymbol {
				NamedTypeSymbol = _currentClassSymbol,
				Category = CategoryType.Return,
				MemberName = methodSymbol?.Name ?? node.Identifier.Text,
				ActualType = s.Item1,
				SubCategory = s.Item2,
			}));

			// 매개변수들
			foreach(var param in node.ParameterList.Parameters)
			{
				var paramSymbol = _semanticModel.GetDeclaredSymbol(param);
				References.AddRange(ExtractReferencedTypes(paramSymbol?.Type).Select(s => new TypeReferenceSymbol {
					NamedTypeSymbol = _currentClassSymbol,
					Category = CategoryType.Parameter,
					MemberName = paramSymbol?.Name ?? param.Identifier.Text,
					ActualType = s.Item1,
					SubCategory = s.Item2,
				}));
			}

			base.VisitMethodDeclaration(node);
		}

		public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
		{
			var typeSymbol = _semanticModel.GetTypeInfo(node.Declaration.Type).Type;

			foreach(var variable in node.Declaration.Variables)
			{
				References.AddRange(ExtractReferencedTypes(typeSymbol).Select(s => new TypeReferenceSymbol {
					NamedTypeSymbol = _currentClassSymbol,
					Category = CategoryType.Local,
					MemberName = variable.Identifier.Text,
					ActualType = s.Item1,
					SubCategory = s.Item2,
				}));
			}

			base.VisitLocalDeclarationStatement(node);
		}

		public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
		{
			var typeSymbol = _semanticModel.GetTypeInfo(node).Type;

			References.AddRange(ExtractReferencedTypes(typeSymbol).Select(s => new TypeReferenceSymbol {
				NamedTypeSymbol = _currentClassSymbol,
				Category = CategoryType.ObjectCreation,
				MemberName = node.Type.ToString(),
				ActualType = s.Item1,
				SubCategory = s.Item2,
			}));

			base.VisitObjectCreationExpression(node);
		}

		// 핵심: List<T>, T[], Dictionary<K,V> 등에서 실제 하위 타입만 추출
		private List<(ITypeSymbol, SubCategoryType)> ExtractReferencedTypes(ITypeSymbol symbol, SubCategoryType categoryType = SubCategoryType.None)
		{
			var list = new List<(ITypeSymbol, SubCategoryType)>();
			if(symbol == null) return list;

			if(symbol is IArrayTypeSymbol arrayType)
			{
				list.AddRange(ExtractReferencedTypes(arrayType.ElementType, SubCategoryType.IsArray));
			}
			else if(symbol is INamedTypeSymbol named && named.IsGenericType)
			{
				foreach(var typeArgument in named.TypeArguments)
				{
					list.AddRange(ExtractReferencedTypes(typeArgument, SubCategoryType.IsGeneric));
				}
			}
			else
			{
				list.Add((symbol, categoryType));
			}

			return list;
		}

		internal void Clear()
		{
			_semanticModel = null;
			_currentClassSymbol = null;
			if(References != null)
			{
				References.ForEach(r => r.Dispose());
				References.Clear();
				References = null;
			}
		}
	}
}
