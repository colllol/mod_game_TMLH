# IL2CPP Metadata Security Scanner

A defensive static-analysis tool that inspects Unity `global-metadata.dat` files for strings that may indicate security-sensitive artifacts shipped in a build. It generates structured JSON and human-readable HTML reports for use in bug bounty and responsible disclosure workflows.

## What This Tool Does

- Reads `global-metadata.dat` offline
- Extracts candidate strings from the metadata string section
- Applies regex rules to detect patterns such as API endpoints, hardcoded credentials, debug commands, internal IP addresses, and connection strings
- Writes a JSON report and an HTML report summarizing findings

## What This Tool Does Not Do

- It does not modify game processes
- It does not inject code, hook functions, or analyze runtime memory
- It does not communicate with any live service on behalf of the target application
- It does not bypass copy-protection or anti-cheat systems

## Legal & Ethical Use

This tool is intended only for defensive security research and responsible disclosure.

Use it only when:

- You own the build
- You have explicit written permission from the copyright owner
- The testing is performed on your own infrastructure or authorized environments

Do not use this tool on third-party software without authorization. Public disclosure should follow vendor policies and applicable laws.

## Build

```bash
cd Il2CppMetadataScanner
dotnet build
```

## Usage

```bash
dotnet run -- --metadata "C:\Games\MyGame\MyGame_Data\il2cpp_data\global-metadata.dat" --output .\reports
```

## Interpreting Reports

- **Critical/High** findings generally require manual validation before disclosure
- String extraction is conservative and may produce false positives
- Verify context before assuming exploitability
- Combine metadata findings with other static review when possible

## Responsible Disclosure Workflow

1. Validate findings and rule out false positives
2. Prepare a concise report with reproduction steps and affected data types
3. Submit findings to the vendor through their security contact or disclosure policy
4. Allow reasonable time for remediation before public disclosure

## Notes on Metadata Formats

Unity metadata formats change between versions. This tool uses a version-tolerant approach: it parses the header, identifies the string section bounds when possible, and scans extracted strings. If parsing fails, inspect the metadata header manually and validate that the input file is a valid `global-metadata.dat`.
