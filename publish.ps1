# Cleanup
Remove-Item  .\ActionCards\bin\Release\netcoreapp3.1\publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item  .\KarthagoCharacterGeneratorTool\bin\Release\netcoreapp3.1\publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item  .\SienceCards\bin\Release\netcoreapp3.1\publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item  .\CrisesCards\bin\Release\netcoreapp3.1\publish -Recurse -Force -ErrorAction SilentlyContinue

# publish
dotnet publish .\ActionCards\ActionCards.csproj /p:PublishProfile=osx
dotnet publish .\ActionCards\ActionCards.csproj /p:PublishProfile=win-x86

dotnet publish .\KarthagoCharacterGeneratorTool\CharacterGenerator.csproj /p:PublishProfile=osx
dotnet publish .\KarthagoCharacterGeneratorTool\CharacterGenerator.csproj /p:PublishProfile=win-x86

dotnet publish .\SienceCards\SienceCards.csproj /p:PublishProfile=osx
dotnet publish .\SienceCards\SienceCards.csproj /p:PublishProfile=win-x86

dotnet publish .\CrisesCards\CrisesCards.csproj /p:PublishProfile=osx
dotnet publish .\CrisesCards\CrisesCards.csproj /p:PublishProfile=win-x86

# Move
Copy-Item  .\ActionCards\bin\Release\netcoreapp3.1\publish\win -Destination C:\Users\patri\Arbeitstitel-Karthago\tools -Recurse -Force
Copy-Item  .\ActionCards\bin\Release\netcoreapp3.1\publish\osx -Destination C:\Users\patri\Arbeitstitel-Karthago\tools -Recurse -Force

Copy-Item  .\KarthagoCharacterGeneratorTool\bin\Release\netcoreapp3.1\publish\win -Destination C:\Users\patri\Arbeitstitel-Karthago\tools -Recurse -Force
Copy-Item  .\KarthagoCharacterGeneratorTool\bin\Release\netcoreapp3.1\publish\osx -Destination C:\Users\patri\Arbeitstitel-Karthago\tools -Recurse -Force

Copy-Item  .\SienceCards\bin\Release\netcoreapp3.1\publish\win -Destination C:\Users\patri\Arbeitstitel-Karthago\tools -Recurse -Force
Copy-Item  .\SienceCards\bin\Release\netcoreapp3.1\publish\osx -Destination C:\Users\patri\Arbeitstitel-Karthago\tools -Recurse -Force

Copy-Item  .\CrisesCards\bin\Release\netcoreapp3.1\publish\win -Destination C:\Users\patri\Arbeitstitel-Karthago\tools -Recurse -Force
Copy-Item  .\CrisesCards\bin\Release\netcoreapp3.1\publish\osx -Destination C:\Users\patri\Arbeitstitel-Karthago\tools -Recurse -Force
