import os
import uuid

def generate_guid():
    """Generate a Unity-compatible GUID"""
    return uuid.uuid4().hex

def create_folder_meta(path, guid):
    """Create .meta file for a folder"""
    meta_content = f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    with open(path + ".meta", "w", encoding="utf-8") as f:
        f.write(meta_content)

def create_file_meta(path, guid):
    """Create .meta file for a C# script"""
    ext = os.path.splitext(path)[1]

    if ext == ".cs":
        meta_content = f"""fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    elif ext == ".json":
        meta_content = f"""fileFormatVersion: 2
guid: {guid}
TextScriptImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    elif ext == ".md":
        meta_content = f"""fileFormatVersion: 2
guid: {guid}
TextScriptImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    elif ext == ".asmdef":
        meta_content = f"""fileFormatVersion: 2
guid: {guid}
AssemblyDefinitionImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    else:
        meta_content = f"""fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

    with open(path + ".meta", "w", encoding="utf-8") as f:
        f.write(meta_content)

def generate_meta_files(root_dir):
    """Generate .meta files for all directories and files"""
    print(f"Generating .meta files for: {root_dir}")

    # Walk through all directories and files
    for dirpath, dirnames, filenames in os.walk(root_dir):
        # Skip .git directory
        if ".git" in dirpath:
            continue

        # Generate .meta for subdirectories
        for dirname in dirnames:
            if dirname == ".git":
                continue
            dir_path = os.path.join(dirpath, dirname)
            meta_path = dir_path + ".meta"

            if not os.path.exists(meta_path):
                guid = generate_guid()
                create_folder_meta(dir_path, guid)
                print(f"Created: {meta_path}")

        # Generate .meta for files
        for filename in filenames:
            if filename.endswith(".meta"):
                continue

            file_path = os.path.join(dirpath, filename)
            meta_path = file_path + ".meta"

            if not os.path.exists(meta_path):
                guid = generate_guid()
                create_file_meta(file_path, guid)
                print(f"Created: {meta_path}")

if __name__ == "__main__":
    framework_path = "D:/CoolGameFramework/Assets/CoolGameFramework"
    generate_meta_files(framework_path)
    print("\n✓ All .meta files generated successfully!")
