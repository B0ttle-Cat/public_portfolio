using System.Collections.Generic;

using UnityEngine;

namespace BC.Base
{
	public partial class Utils//.Random
	{
		public static T RandomInList<T>(this List<T> list)
		{
			int count = list.Count;
			if(count == 0) return default;

			int index = UnityEngine.Random.Range(0, count);
			return list[index];
		}
		public static int RandomIndex<T>(this List<T> list)
		{
			int count = list.Count;
			if(count == 0) return -1;

			return UnityEngine.Random.Range(0, count);
		}

		/// <summary>
		/// ������ probability Ȯ���� ��ġ�� true �� ��ȯ�ϴ� �Լ�
		/// offset�� �⺻ 0 �̴�. + ���� Ȯ���� �ø���, - �� Ȯ���� �����. 
		/// probability + offset �� 1���� ũ�� ������ ���̴�.
		/// probability + offset �� 1���� ������ ������ �����̴�.
		/// </summary>
		public static bool BernoulliTrial(float probability, float offset = 0f)
		{
			return probability + offset > Random.value;
		}

		/// <summary>
		/// A�� B�� ���� ���� ��ȯ�մϴ�.
		/// ���Ѵ밡 ���Ե� ��� �����ϰ� ó���մϴ�.
		/// A / B�� 0���� ������ 0���� ��ȯ�մϴ�.
		/// </summary>
		public static float Probability_A_in_B(float A, float B)
		{
			if(A<0f) A = 0f;
			if(B<0f) B = 0f;
			if(Mathf.Approximately(A, 0f)) A = 0f;
			if(Mathf.Approximately(B, 0f)) B = 0f;
			bool isPositiveInfinity_A = float.IsPositiveInfinity(A);
			bool isPositiveInfinity_B = float.IsPositiveInfinity(B);

			//���Ѵ� ó��
			if(isPositiveInfinity_A)
			{
				return 1f;
			}
			if(isPositiveInfinity_B)
			{
				return 0f;
			}

			// 0�� ��� ó��
			return B == 0f
				? (A == 0f) ? 0.5f : 1f
				: (A == 0f) ? 0f : A / B;
		}

		/// <summary>
		/// A�� B�� �� �տ��� A�� �����ϴ� ������ ��ȯ�մϴ�.
		/// </summary>
		public static float Probability_A_in_sumAB(float A, float B)
		{
			if(A<0f) A = 0f;
			if(B<0f) B = 0f;
			bool isPositiveInfinity_A = float.IsPositiveInfinity(A);
			bool isPositiveInfinity_B = float.IsPositiveInfinity(B);

			if(isPositiveInfinity_A && isPositiveInfinity_B)
			{
				// �� �� ���Ѵ��� ��� ���ǵ��� �����Ƿ� ���ܸ� �����ų� �⺻�� ��ȯ
				return 0.5f; // �Ǵ� throw new ArgumentException("Both A and other are infinite.");
			}
			if(isPositiveInfinity_A)
			{
				// A�� ���Ѵ��̰� B�� ������ ��� A�� ������ 1
				return 1f;
			}
			if(isPositiveInfinity_B)
			{
				// B�� ���Ѵ��̰� A�� ������ ��� A�� ������ 0
				return 0f;
			}

			// A�� B�� ������ ���
			float total = A + B;

			if(Mathf.Approximately(total, 0f))
			{
				// A�� B�� ���� 0�� ����� ��� ������ 0.5�� ��ȯ
				return 0.5f;
			}
			if(Mathf.Approximately(A, B))
			{
				// A�� Other�� ���� ���� ��
				return 0.5f;
			}
			return A / total;
		}
		/// <summary>
		/// A�� other �迭�� �� �տ��� A�� �����ϴ� ������ ��ȯ�մϴ�.
		/// </summary>
		public static float Probability_A_in_Total(float A, params float[] other)
		{
			int length = other.Length;
			if(length == 1)
			{
				return Probability_A_in_sumAB(A, other[0]);
			}
			if(A<0f) A = 0f;

			// ���Ѵ� ���� �˻�
			bool isPositiveInfinity_A = float.IsPositiveInfinity(A);
			bool isPositiveInfinity_Other = false;

			// other �迭 �� ���Ѵ� ���� �˻�
			for(int o = 0 ; o < length ; o++)
			{
				if(float.IsPositiveInfinity(other[o]))
				{
					isPositiveInfinity_Other = true;
					break;
				}
			}

			if(isPositiveInfinity_A && isPositiveInfinity_Other)
			{
				// �� �� ���Ѵ��� ��� ���ǵ��� �����Ƿ� ���ܸ� �����ų� �⺻�� ��ȯ
				return 0.5f; // �Ǵ� throw new ArgumentException("Both A and other are infinite.");
			}
			if(isPositiveInfinity_A)
			{
				// A�� ���Ѵ��̰� B�� ������ ��� A�� ������ 1
				return 1f;
			}
			if(isPositiveInfinity_Other)
			{
				// other�� ���Ѵ��̰� A�� ������ ��� A�� ������ 0
				return 0f;
			}

			// ��� other�� ������ ���
			float sumOther = 0f;
			foreach(var v in other)
			{
				sumOther += v;
			}
			if(sumOther<0f) sumOther = 0f;
			float total = A + sumOther;

			// A�� Other�� ������ 0�� ��� 0.5 ��ȯ
			if(Mathf.Approximately(total, 0f))
			{
				return 0.5f;
			}

			// A�� Other�� ���հ� ���� ��
			if(Mathf.Approximately(A, sumOther))
			{
				return 0.5f;
			}

			// �Ϲ����� ��� A / (A + sumOther)
			return A / total;
		}


		public static class RandomDistribution
		{
			/// <summary><code>
			/// ���� ���� ��ǥ�� X �� ��� (-1~1)(���� 0 ���� ��ħ)
			/// </code></summary>
			public static float NextSemicircular()
			{
				return Random.insideUnitCircle.x;
			}
			/// <summary>
			/// ������� ������ ���Ժ����� ����Ű�� �Լ��Դϴ�.
			/// NextGaussian(0f, 1f); �� �����մϴ�.
			/// </summary>
			public static float NextGaussianNormal()
			{
				return NextGaussian(0f, 1f);
			}
			/// <summary>
			/// ������� ���� �Լ��Դϴ�.
			/// �߾ӿ� �е��� ����, �������� ������ �������ϴ�.
			/// ������ �̷л� ���Ѵ�� �л�˴ϴ�.
			/// </summary>
			public static float NextGaussian(float mean = 0f, float stdDev = 1f)
			{
				// Box-Muller ��ȯ�� ����Ͽ� ǥ�� ���� ���� �� ����
				float u1 = 1.0f - UnityEngine.Random.value; // (0,1] ������ �յ� ����
				float u2 = 1.0f - UnityEngine.Random.value;
				float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2); // ǥ�� ���� ���� ��

				// ��հ� ǥ�������� �����Ͽ� ���ϴ� ���� ���� �� ����
				return mean + stdDev * randStdNormal;
			}
			/// <summary>
			/// -1 ~ 1 ������ ���� �������� ����&������ ������� ���� �Լ��Դϴ�.
			/// �߾ӿ� �е��� ����, �������� ������ �������ϴ�.
			/// NextGaussian(0f, 0.25f); �� �����մϴ�.
			/// </summary>
			public static float NextGaussianSize1()
			{
				return Mathf.Clamp(NextGaussian(0f, 0.25f), -1f, 1f);
			}
			/// <summary>
			/// 0 ~ 1 ������ ���� �������� ����&������ ������� ���� �Լ��Դϴ�.
			/// �߾ӿ� �е��� ����, �������� ������ �������ϴ�.
			/// NextGaussian(0f, 0.25f); �� �����մϴ�.
			/// </summary>
			public static float NextGaussianClamp1()
			{
				return (Mathf.Clamp(NextGaussian(0f, 0.25f), -1f, 1f) + 1f) * 0.5f;
			}
			/// <summary>
			/// ���� ���� �Դϴ�.
			/// ���� Ŭ���� �󵵰� �������ϴ�.
			/// </summary>
			/// <param name="lambda"></param>
			/// <returns></returns>
			public static float NextExponential(float lambda)
			{
				float u = Random.Range(0f, 1f);
				return -Mathf.Log(1f - u) / lambda;
			}

			/// <summary>
			/// �ﰢ ������ ������ ������ float ���� �����մϴ�.
			/// </summary>
			/// <param name="min">�ּҰ�</param>
			/// <param name="mode">�ֺ�</param>
			/// <param name="max">�ִ밪</param>
			/// <returns>�ﰢ ������ ������ ������ float ��</returns>
			public static float NextTriangular(float mode = 0f, float min = -1f, float max = 1f)
			{
				float u = Random.Range(0f, 1f);
				float c = (mode - min) / (max - min);

				if(u < c)
				{
					return min + Mathf.Sqrt(u * (max - min) * (mode - min));
				}
				else
				{
					return max - Mathf.Sqrt((1f - u) * (max - min) * (max - mode));
				}
			}
			/// <summary>
			/// ��Ÿ ������ ������ ���� ���� �����մϴ�.
			/// </summary>
			/// <param name="alpha">���� ������ ��� �� (�� > 0)(1 ���� ũ�� ���� 1 ������ �о���� �� 1���� ������ ���� 0 ���� ���������� ��)</param>
			/// <param name="beta">���� ������ ��� �� (�� > 0)(1 ���� ũ�� ���� 0 ������ �о���� �� 1���� ������ ���� 1 ���� ���������� ��)</param>
			/// alpha / beta �� �Ѵ� 1 �̸� Random.value �� ������.
			/// <returns>��Ÿ ������ ������ ���� �� (0 ~ 1)</returns>
			public static float NextBeta(float alpha, float beta)
			{
				// �� 0.03 ���� ������ ������ ����
				if(alpha <= 0.03f) alpha = 0.03f;
				if(beta <= 0.03f) beta = 0.03f;

				float gamma1 = NextGamma(alpha, 1f);
				float gamma2 = NextGamma(beta, 1f);
				return gamma1 / (gamma1 + gamma2);
			}

			/// <summary>
			/// ���� ������ ������ ���� ���� �����մϴ�.
			/// </summary>
			/// <param name="alpha">���� ������ ��� �� (�� > 0)</param>
			/// <param name="beta">���� ������ ��� �� (�� > 0)</param>
			/// <returns>���� ������ ������ ���� ��</returns>
			public static float NextGamma(float alpha, float beta)
			{
				if(alpha <= 0.00000001f) alpha = 0.00000001f;
				if(beta <= 0.00000001f) beta = 0.00000001f;

				if(alpha < 1)
				{
					// �� < 1�� ���, �� + 1�� �����ϰ� ����� ����
					float u = Random.value;
					return NextGamma(alpha + 1, beta) * Mathf.Pow(u, 1f / alpha);
				}

				float d = alpha - 1f / 3f;
				float c = 1f / Mathf.Sqrt(9f * d);
				while(true)
				{
					float x = NextGaussianNormal();
					float v = 1f + c * x;
					if(v <= 0)
						continue;
					v = v * v * v;
					float u = Random.value;
					float y = x * x;
					if(u < 1f - 0.0331f * y * y)
						return d * v / beta;
					if(Mathf.Log(u) < 0.5f * y + d * (1f - v + Mathf.Log(v)))
						return d * v / beta;
				}
			}
		}

	}
}
