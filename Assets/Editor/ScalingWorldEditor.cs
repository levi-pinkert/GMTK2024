using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScalingWorld))]
public class ScalingWorldEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if(GUILayout.Button("Apply materials to all"))
		{
			ApplyMaterialsToTaggedObjects();
		}
	}

	private void ApplyMaterialsToTaggedObjects()
	{
		ScalingWorld scalingWorld = (ScalingWorld)target;
		GameManager gameManager = FindObjectOfType<GameManager>();
		Assert.IsNotNull(gameManager);

		Dictionary<string, WorldMaterialType> materialTypes = new();
		foreach (WorldMaterialType matType in gameManager.worldMaterialTypes)
		{
			materialTypes.Add(matType.tagName, matType);
		}

		Collider[] colliders = scalingWorld.GetComponentsInChildren<Collider>();
		foreach (Collider collider in colliders)
		{
			if (materialTypes.TryGetValue(collider.gameObject.tag, out WorldMaterialType matType))
			{
				if (matType != null && matType.physicMaterial != null)
				{
					collider.material = matType.physicMaterial;
					EditorUtility.SetDirty(collider.gameObject);
				}
			}
		}

		MeshRenderer[] meshRenderers = scalingWorld.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			if (materialTypes.TryGetValue(meshRenderer.gameObject.tag, out WorldMaterialType matType))
			{
				if (matType != null && matType.renderMaterial != null)
				{
					meshRenderer.material = matType.renderMaterial;
					EditorUtility.SetDirty(meshRenderer.gameObject);
				}
			}
		}
	}
}
