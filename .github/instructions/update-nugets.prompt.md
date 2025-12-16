---
agent: 'agent'
description: 'Update NuGet packages'
---

# Update NuGet Packages

Update the NuGet package versions for the entire solution.

The project is using Centralized Package Management, so make sure to update the versions in the Directory.Packages.props file.
When possible use shared MSBuild properties declared at the top of the Directory.Packages.props file for related packages that should be versioned together.

Never downgrade packages. 
It is unacceptable to leave packages outdated after completing this task.

When updating Entity Framework or Kiota packages also update the dotnet-tools.json with a matching version.

## NuGet MCP Server Tools

Use the NuGet MCP server tools to query package versions and perform updates:

- **`mcp_nuget_get-latest-package-version`** - Query the latest version of a specific NuGet package. Use this to check for updates to individual packages. Set `includePrerelease` based on whether the current package version is a preview.
- **`mcp_nuget_get-nuget-solver-latest-versions`** - Use NuGetSolver to update all packages to their latest compatible versions. This ensures dependency compatibility across the solution.
- **`mcp_nuget_update-package-to-version`** - Update specific packages to exact versions using NuGetSolver for compatibility checking.

When using these tools:
- Always provide the `solutionDirectory` parameter pointing to the solution root.
- Provide `projectPaths` as an array of absolute paths to all `.csproj` files in the solution.
- Set `includePrerelease: false` for stable packages, `includePrerelease: true` for packages already on preview versions.
- Set `includeVulnerable: false` to exclude vulnerable package versions.

## Process

1. Examine the Directory.Packages.props file to identify all top-level NuGet packages and their current versions.
2. Examine the NuGet.config file to identify all package sources.
3. Use `mcp_nuget_get-nuget-solver-latest-versions` to determine the latest compatible versions for all packages in the solution.
4. For packages on preview versions, use `mcp_nuget_get-latest-package-version` with `includePrerelease: true` to find the latest preview version.
5. Update the version numbers in the Directory.Packages.props file accordingly.
6. For any non-top-level packages that are no longer needed (i.e., not referenced by any project), remove them from the Directory.Packages.props file.
7. For packages that have a version override specified in a project file, prompt to see if the override should be removed to use the centralized version instead.
8. Update all dotnet local tools to their latest versions using `dotnet tool update`.
9. Re-check to verify that no NuGet packages are outdated after making the updates by querying versions again.
10. Run a build and tests to ensure that everything works correctly after the updates.