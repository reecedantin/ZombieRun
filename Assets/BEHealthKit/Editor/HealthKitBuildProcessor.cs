using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Build;
using System;
using System.Collections;
using System.IO;
using UnityEditor.iOS.Xcode;
using BeliefEngine.HealthKit;

/*! @brief 		Build processor script.
	@details	This build processor updates the Xcode project in order to build automatically. It adds the HealthKit capability and frameworks, and creates an
				entitlements file.
				It also scans through the scenes in the Unity project looking for a HealthKitDataTypes object, and extracts the usage / update strings from it,
				which are used to present an alert to the user when requesting permission.
 */
public class HealthKitBuildProcessor : IPostprocessBuild, IProcessScene
{
	private string shareString = null;
	private string updateString = null;
	
	/*! @brief required by the IPostprocessBuild interface. Set high to let other postprocess scripts run first. */
	public int callbackOrder {
		get { return 100; }
	}

	/*! @brief        Searches for HealthKitDataTypes objects & reads the usage strings for the OnPostprocessBuild phase. 
		@param scene  the scene being processed.
	 */
	public void OnProcessScene(Scene scene) {
		GameObject[] rootObjects = scene.GetRootGameObjects();
		foreach (GameObject obj in rootObjects) {
			HealthKitDataTypes types = obj.GetComponentInChildren<HealthKitDataTypes>();
			if (types != null) {
				if (types.AskForSharePermission()) {
					this.shareString = types.healthShareUsageDescription;
				}

				if (types.AskForUpdatePermission()) {
					this.updateString = types.healthUpdateUsageDescription;
				}
			}
		}
	}

	/*! @brief              Updates the Xcode project. 
		@param buildTarget  the target build platform
		@param path         the path of the target build
	 */
	public void OnPostprocessBuild(BuildTarget buildTarget, string path) {
		if (buildTarget == BuildTarget.iOS) {
			string plistPath = Path.Combine(path, "Info.plist");
			PlistDocument info = GetInfoPlist(plistPath);
			PlistElementDict rootDict = info.root;
			// // Add the keys
			if (this.shareString != null) {
				rootDict.SetString("NSHealthShareUsageDescription", this.shareString);
			} 
//			else {
//				Debug.LogError("unable to read NSHealthShareUsageDescription");
//			}
			if (this.updateString != null) {
				rootDict.SetString("NSHealthUpdateUsageDescription", this.updateString);
			}

			// Write the file
			info.WriteToFile(plistPath);

			string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

			PBXProject proj = new PBXProject();
			proj.ReadFromFile(projPath);

			string targetName = PBXProject.GetUnityTargetName();
			string target = proj.TargetGuidByName(targetName);


			// Entitlements
			//--------------
			string projectname = GetProjectName(info);
			string entitlementsFile = Path.ChangeExtension(projectname, "entitlements");
			string entitlementsPath = Path.Combine(path, entitlementsFile);
			var dst = Path.Combine(projPath, entitlementsPath);
			CreateEntitlements(dst);
			proj.AddFileToBuild(target, proj.AddFile(entitlementsPath, entitlementsPath, PBXSourceTree.Source));
			proj.AddBuildProperty(target, "CODE_SIGN_ENTITLEMENTS", entitlementsPath);

			// add HealthKit capability
			ProjectCapabilityManager capabilities = new ProjectCapabilityManager(projPath, entitlementsPath, targetName);
			capabilities.AddHealthKit();

			// add HealthKit Framework
			proj.AddFrameworkToProject(target, "HealthKit.framework", true);

			// Set a custom link flag
			proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");

			proj.WriteToFile(projPath);
		}
	}

	// -------------------------------

	internal static void CopyAndReplaceDirectory(string srcPath, string dstPath) {
		if (Directory.Exists(dstPath)) Directory.Delete(dstPath);
		if (File.Exists(dstPath)) File.Delete(dstPath);

		Directory.CreateDirectory(dstPath);

		foreach (var file in Directory.GetFiles(srcPath)) {
			File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
		}

		foreach (var dir in Directory.GetDirectories(srcPath)) {
			CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
		}
	}

	internal static void CreateEntitlements(string destinationPath) {
		if (!System.IO.File.Exists(destinationPath)) {
			try {
				System.IO.File.WriteAllText(destinationPath,
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
	<key>com.apple.developer.healthkit</key>
	<true/>
</dict>
</plist>");
			}
			catch (Exception e) {
				Debug.LogErrorFormat("error writing to file: {0}", e);
			}
		} else {
			Debug.LogWarningFormat("File \"{0}\" already exists.", destinationPath);
		}
	}

	internal static PlistDocument GetInfoPlist(string plistPath) {
		// Get the plist file
		PlistDocument plist = new PlistDocument();
		plist.ReadFromFile(plistPath);
		return plist;
	}
	
	internal static string GetProjectName(PlistDocument plist) {
		string projectname = plist.root["CFBundleDisplayName"].AsString();
		return projectname;
	}
}
