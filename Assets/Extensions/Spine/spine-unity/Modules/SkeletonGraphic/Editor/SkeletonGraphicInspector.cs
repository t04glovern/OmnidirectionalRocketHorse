/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using UnityEditor;
using Spine;

namespace Spine.Unity.Editor {

	[InitializeOnLoad]
	[CustomEditor(typeof(SkeletonGraphic))]
	[CanEditMultipleObjects]
	public class SkeletonGraphicInspector : UnityEditor.Editor {
		SerializedProperty material_, color_;
		SerializedProperty skeletonDataAsset_, initialSkinName_;
		SerializedProperty startingAnimation_, startingLoop_, timeScale_, freeze_, unscaledTime_, tintBlack_;
		SerializedProperty meshGeneratorSettings_;
		SerializedProperty raycastTarget_;

		SkeletonGraphic thisSkeletonGraphic;

		void OnEnable () {
			var so = this.serializedObject;
			thisSkeletonGraphic = target as SkeletonGraphic;

			// MaskableGraphic
			material_ = so.FindProperty("m_Material");
			color_ = so.FindProperty("m_Color");
			raycastTarget_ = so.FindProperty("m_RaycastTarget");

			// SkeletonRenderer
			skeletonDataAsset_ = so.FindProperty("skeletonDataAsset");
			initialSkinName_ = so.FindProperty("initialSkinName");
			//tintBlack_ = so.FindProperty("tintBlack");

			// SkeletonAnimation
			startingAnimation_ = so.FindProperty("startingAnimation");
			startingLoop_ = so.FindProperty("startingLoop");
			timeScale_ = so.FindProperty("timeScale");
			unscaledTime_ = so.FindProperty("unscaledTime");
			freeze_ = so.FindProperty("freeze");

			meshGeneratorSettings_ = so.FindProperty("meshGenerator").FindPropertyRelative("settings");
			meshGeneratorSettings_.isExpanded = SkeletonRendererInspector.advancedFoldout;
		}

		public override void OnInspectorGUI () {
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(skeletonDataAsset_);
			EditorGUILayout.PropertyField(material_);
			EditorGUILayout.PropertyField(color_);

			if (thisSkeletonGraphic.skeletonDataAsset == null) {
				EditorGUILayout.HelpBox("You need to assign a SkeletonDataAsset first.", MessageType.Info);
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				return;
			}
			using (new SpineInspectorUtility.BoxScope()) {
				EditorGUILayout.PropertyField(meshGeneratorSettings_, SpineInspectorUtility.TempContent("Advanced..."), includeChildren: true);
				SkeletonRendererInspector.advancedFoldout = meshGeneratorSettings_.isExpanded;
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(initialSkinName_);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(startingAnimation_);
			EditorGUILayout.PropertyField(startingLoop_);
			EditorGUILayout.PropertyField(timeScale_);
			EditorGUILayout.PropertyField(unscaledTime_, SpineInspectorUtility.TempContent(unscaledTime_.displayName, tooltip: "If checked, this will use Time.unscaledDeltaTime to make this update independent of game Time.timeScale. Instance SkeletonGraphic.timeScale will still be applied."));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(freeze_);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(raycastTarget_);

			bool wasChanged = EditorGUI.EndChangeCheck();

			if (wasChanged)
				serializedObject.ApplyModifiedProperties();
		}

		#region Menus
		[MenuItem("CONTEXT/SkeletonGraphic/Match RectTransform with Mesh Bounds")]
		static void MatchRectTransformWithBounds (MenuCommand command) {
			var skeletonGraphic = (SkeletonGraphic)command.context;
			Mesh mesh = skeletonGraphic.GetLastMesh();
			if (mesh == null) {
				Debug.Log("Mesh was not previously generated.");
				return;
			}

			if (mesh.vertexCount == 0) {
				skeletonGraphic.rectTransform.sizeDelta = new Vector2(50f, 50f);
				skeletonGraphic.rectTransform.pivot = new Vector2(0.5f, 0.5f);
				return;
			}

			mesh.RecalculateBounds();
			var bounds = mesh.bounds;
			var size = bounds.size;
			var center = bounds.center;
			var p = new Vector2(
				        0.5f - (center.x / size.x),
				        0.5f - (center.y / size.y)
			        );

			skeletonGraphic.rectTransform.sizeDelta = size;
			skeletonGraphic.rectTransform.pivot = p;
		}

		[MenuItem("GameObject/Spine/SkeletonGraphic (UnityUI)", false, 15)]
		static public void SkeletonGraphicCreateMenuItem () {
			var parentGameObject = Selection.activeObject as GameObject;
			var parentTransform = parentGameObject == null ? null : parentGameObject.GetComponent<RectTransform>();

			if (parentTransform == null)
				Debug.LogWarning("Your new SkeletonGraphic will not be visible until it is placed under a Canvas");

			var gameObject = NewSkeletonGraphicGameObject("New SkeletonGraphic");
			gameObject.transform.SetParent(parentTransform, false);
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = gameObject;
			EditorGUIUtility.PingObject(Selection.activeObject);
		}

		// SpineEditorUtilities.InstantiateDelegate. Used by drag and drop.
		public static Component SpawnSkeletonGraphicFromDrop (SkeletonDataAsset data) {
			return InstantiateSkeletonGraphic(data);
		}

		public static SkeletonGraphic InstantiateSkeletonGraphic (SkeletonDataAsset skeletonDataAsset, string skinName) {
			return InstantiateSkeletonGraphic(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
		}

		public static SkeletonGraphic InstantiateSkeletonGraphic (SkeletonDataAsset skeletonDataAsset, Skin skin = null) {
			string spineGameObjectName = string.Format("SkeletonGraphic ({0})", skeletonDataAsset.name.Replace("_SkeletonData", ""));
			var go = NewSkeletonGraphicGameObject(spineGameObjectName);
			var graphic = go.GetComponent<SkeletonGraphic>();
			graphic.skeletonDataAsset = skeletonDataAsset;

			SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

			if (data == null) {
				for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
					string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
					skeletonDataAsset.atlasAssets[i] = (AtlasAsset)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAsset));
				}

				data = skeletonDataAsset.GetSkeletonData(true);
			}

			skin = skin ?? data.DefaultSkin ?? data.Skins.Items[0];
			graphic.MeshGenerator.settings.zSpacing = SpineEditorUtilities.defaultZSpacing;

			graphic.Initialize(false);
			graphic.Skeleton.SetSkin(skin);
			graphic.initialSkinName = skin.Name;
			graphic.Skeleton.UpdateWorldTransform();
			graphic.UpdateMesh();

			return graphic;
		}

		static GameObject NewSkeletonGraphicGameObject (string gameObjectName) {
			var go = new GameObject(gameObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(SkeletonGraphic));
			var graphic = go.GetComponent<SkeletonGraphic>();
			graphic.material = SkeletonGraphicInspector.DefaultSkeletonGraphicMaterial;
			return go;
		}

		public static Material DefaultSkeletonGraphicMaterial {
			get {
				var guids = AssetDatabase.FindAssets("SkeletonGraphicDefault t:material");
				if (guids.Length <= 0) return null;

				var firstAssetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
				if (string.IsNullOrEmpty(firstAssetPath)) return null;

				var firstMaterial = AssetDatabase.LoadAssetAtPath<Material>(firstAssetPath);
				return firstMaterial;
			}
		}

		#endregion
	}
}
