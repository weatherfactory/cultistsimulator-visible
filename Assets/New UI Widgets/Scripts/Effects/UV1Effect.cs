namespace UIWidgets
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;
	using UnityEngine.UI;

	/// <summary>
	/// UV1 effect.
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(Graphic))]
	public abstract class UV1Effect : MonoBehaviour, IMaterialModifier, IMeshModifier
	{
		/// <summary>
		/// Shader.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("RippleShader")]
		protected Shader EffectShader;

		/// <summary>
		/// Base material.
		/// </summary>
		[NonSerialized]
		protected Material BaseMaterial;

		/// <summary>
		/// Ring material.
		/// </summary>
		[NonSerialized]
		protected Material EffectMaterial;

		[NonSerialized]
		Graphic graphic;

		/// <summary>
		/// Graphic component.
		/// </summary>
		protected Graphic Graphic
		{
			get
			{
				if (graphic == null)
				{
					graphic = GetComponent<Graphic>();
				}

				return graphic;
			}
		}

		[NonSerialized]
		RectTransform rectTransform;

		/// <summary>
		/// RectTransform component.
		/// </summary>
		protected RectTransform RectTransform
		{
			get
			{
				if (rectTransform == null)
				{
					rectTransform = transform as RectTransform;
				}

				return rectTransform;
			}
		}

		/// <summary>
		/// Validate this instance.
		/// </summary>
		protected virtual void OnValidate()
		{
			UpdateMaterial();
		}

		/// <summary>
		/// Modify mesh.
		/// </summary>
		/// <param name="mesh">Mesh.</param>
		public virtual void ModifyMesh(Mesh mesh)
		{
		}

		/// <summary>
		/// Add uv1 to the mesh.
		/// </summary>
		/// <param name="verts">Vertex helper.</param>
		public virtual void ModifyMesh(VertexHelper verts)
		{
			var vertex = default(UIVertex);

			// get min and max position to calculate uv1
			verts.PopulateUIVertex(ref vertex, 0);
			var min_x = vertex.position.x;
			var max_x = min_x;
			var min_y = vertex.position.y;
			var max_y = min_y;

			for (int i = 1; i < verts.currentVertCount; i++)
			{
				verts.PopulateUIVertex(ref vertex, i);

				min_x = Math.Min(min_x, vertex.position.x);
				max_x = Math.Max(max_x, vertex.position.x);

				min_y = Math.Min(min_y, vertex.position.y);
				max_y = Math.Max(max_y, vertex.position.y);
			}

			// set uv1 for the ring shader
			var width = max_x - min_x;
			var height = max_y - min_y;
			var size = RectTransform.rect.size;
			var aspect_ratio = size.x / size.y;

			for (int i = 0; i < verts.currentVertCount; i++)
			{
				verts.PopulateUIVertex(ref vertex, i);

				vertex.uv1 = new Vector2(
					(vertex.position.x - min_x) / width,
					(vertex.position.y - min_y) / height / aspect_ratio);

				verts.SetUIVertex(vertex, i);
			}
		}

		/// <summary>
		/// Init material.
		/// </summary>
		protected virtual void InitMaterial()
		{
			SetMaterialProperties();
		}

		/// <summary>
		/// Set material properties.
		/// </summary>
		protected abstract void SetMaterialProperties();

		/// <summary>
		/// Update material.
		/// </summary>
		protected virtual void UpdateMaterial()
		{
			SetMaterialProperties();

			if (EffectMaterial != null)
			{
				Graphic.SetMaterialDirty();
			}
		}

		/// <summary>
		/// Get modified material.
		/// </summary>
		/// <param name="newBaseMaterial">Base material.</param>
		/// <returns>Modified material.</returns>
		public virtual Material GetModifiedMaterial(Material newBaseMaterial)
		{
			if ((BaseMaterial != null) && (newBaseMaterial.GetInstanceID() == BaseMaterial.GetInstanceID()))
			{
				return EffectMaterial;
			}

			if (EffectMaterial != null)
			{
#if UNITY_EDITOR
				DestroyImmediate(EffectMaterial);
#else
				Destroy(EffectMaterial);
#endif
			}

			BaseMaterial = newBaseMaterial;
			EffectMaterial = new Material(newBaseMaterial)
			{
				shader = EffectShader,
			};
			InitMaterial();

			return EffectMaterial;
		}
	}
}