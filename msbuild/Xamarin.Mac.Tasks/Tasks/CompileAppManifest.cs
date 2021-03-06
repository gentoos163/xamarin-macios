﻿//
// CompileAppManifestTask.cs
//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright 2014 Xamarin Inc.

using System;
using System.IO;

using Microsoft.Build.Utilities;

using Xamarin.MacDev;
using Xamarin.MacDev.Tasks;

namespace Xamarin.Mac.Tasks
{
	public class CompileAppManifest : CompileAppManifestTaskBase
	{
		public bool IsXPCService { get; set; }

		public override bool Execute ()
		{
			PDictionary plist;

			try {
				plist = PDictionary.FromFile (AppManifest);
			} catch (Exception ex) {
				LogAppManifestError ("Error loading '{0}': {1}", AppManifest, ex.Message);
				return false;
			}

			plist[ManifestKeys.CFBundleExecutable] = new PString (AssemblyName);

			plist.SetIfNotPresent (ManifestKeys.CFBundleIdentifier, BundleIdentifier);
			plist.SetIfNotPresent (ManifestKeys.CFBundleInfoDictionaryVersion, "6.0");
			if (!IsAppExtension || (IsAppExtension && IsXPCService))
				plist.SetIfNotPresent ("MonoBundleExecutable", AssemblyName + ".exe");
			plist.SetIfNotPresent (ManifestKeys.CFBundleExecutable, AssemblyName);
			plist.SetIfNotPresent (ManifestKeys.CFBundleName, AppBundleName);
			plist.SetIfNotPresent (ManifestKeys.CFBundlePackageType, IsAppExtension ? "XPC!" : "APPL");
			plist.SetIfNotPresent (ManifestKeys.CFBundleSignature, "????");
			plist.SetIfNotPresent (ManifestKeys.CFBundleVersion, "1.0");

			// Merge with any partial plists generated by the Asset Catalog compiler...
			MergePartialPlistTemplates (plist);

			CompiledAppManifest = new TaskItem (Path.Combine (AppBundleDir, "Contents", "Info.plist"));
			plist.Save (CompiledAppManifest.ItemSpec, true, true);

			return !Log.HasLoggedErrors;
		}
	}
}
