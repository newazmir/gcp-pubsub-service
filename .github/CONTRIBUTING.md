# Checkatrade dotnet package contributing guide


## New Contributors

See the [README](/README.md) to get an overview of the project and usage guidelines.

Changes to the repository should be made by Pull Request to the `main` branch

## Github Workflow

toolshed packages should use the [standard workflow](workflows/PublishPackage.yml) where possible.

Changes to workflow must include the following steps as a minimum
- Linting
- Build
- Run Tests
- Publish Package to Github Package repository

## Pull Requests

Pull requests will require approval from a member of the [Codeowners](CODEOWNERS) before changes can be merged in.

## Documentation

When making changes please ensure the following are updated

- [package.json](/package.json) update version number
- [CHANGELOG](/CHANGELOG.md) to detail changes made in new package version
- [README](/README.md) with any changes to usage or new functionality
