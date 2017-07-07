echo 'Welcome to use this pack script, I will pack the Ap.Web project'

echo 'All existing nuget packages as following:'
ls ../../artifacts/Ap.Web*.nupkg | cut -d / -f 4 |  sort -r

echo 'Please input the version id for new package:'
read VERID

echo "The new version id is $VERID"

echo "Packing..."

dotnet pack -c Release -o ../../artifacts --include-symbols --version-suffix $VERID