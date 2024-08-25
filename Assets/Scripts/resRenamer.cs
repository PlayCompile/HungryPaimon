using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class resRenamer : MonoBehaviour
{
#if UNITY_EDITOR
    public string prefabFolderPath; // specify the path to the folder containing prefabs
    public List<GameObject> prefabs = new List<GameObject>();

    private void OnEnable()
    {
        // Get all prefabs in the specified folder path
        string[] assetGuids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabFolderPath });
        foreach (string assetGuid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        StartCoroutine("renameResources");

    }
    IEnumerator renameResources()
    {
        int index = 0;

        while (index < prefabs.Count)
        {
            GameObject prefab = prefabs[index];

            if (prefab != null)
            {
                // Get all materials in the prefab
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material.name.StartsWith(prefab.name + "_"))
                        {
                            // If the material has already been renamed, skip it
                            continue;
                        }

                        // Get the path of the material asset
                        string path = AssetDatabase.GetAssetPath(material);

                        // Rename the material to include the prefab name as a prefix
                        string newName = prefab.name + "_" + material.name;
                        AssetDatabase.RenameAsset(path, newName);
                        EditorUtility.SetDirty(material); // mark the material as dirty to save changes
                    }
                }

                // Save the changes to the modified materials
                AssetDatabase.SaveAssets();
                Debug.Log(index + @"/" + prefabs.Count + " " + prefab.name + " materials renamed.");

                // Get all meshes in the prefab
                MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);

                foreach (MeshFilter meshFilter in meshFilters)
                {
                    Mesh mesh = meshFilter.sharedMesh;
                    if (mesh != null && !mesh.name.StartsWith(prefab.name + "_"))
                    {
                        // Get the path of the mesh asset
                        string path = AssetDatabase.GetAssetPath(mesh);

                        // Rename the mesh to include the prefab name as a prefix
                        string newName = prefab.name + "_" + mesh.name;
                        AssetDatabase.RenameAsset(path, newName);
                        EditorUtility.SetDirty(mesh); // mark the mesh as dirty to save changes
                    }
                }

                // Save the changes to the modified meshes
                AssetDatabase.SaveAssets();
                Debug.Log(index + @"/" + prefabs.Count + " " + prefab.name + " meshes renamed");

                // Get all textures used by materials in the prefab
                Renderer[] renderers2 = prefab.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers2)
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material != null)
                        {
                            string[] textureNames = material.GetTexturePropertyNames();

                            foreach (string textureName in textureNames)
                            {
                                Texture texture = material.GetTexture(textureName);

                                if (texture != null && !texture.name.StartsWith(prefab.name + "_"))
                                {
                                    // Get the path of the texture asset
                                    string path = AssetDatabase.GetAssetPath(texture);

                                    // Rename the texture to include the prefab name as a prefix
                                    string newName = prefab.name + "_" + texture.name;
                                    AssetDatabase.RenameAsset(path, newName);
                                    EditorUtility.SetDirty(texture); // mark the texture as dirty to save changes
                                }
                            }
                        }
                    }
                }

                // Save the changes to the modified textures
                AssetDatabase.SaveAssets();
                Debug.Log(index + @"/" + prefabs.Count + " " + prefab.name + " textures renamed.");
            }

            yield return new WaitForEndOfFrame();

            index++;
        }

        this.gameObject.SetActive(false);
        yield return null;
    }
#endif
}