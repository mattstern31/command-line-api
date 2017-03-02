set -e

SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ "$SOURCE" != /* ]] && SOURCE="$DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
REPOROOT="$DIR"
export NUGET_PACKAGES=$REPOROOT/.nuget
export NUGET_HTTP_CACHE_PATH=$REPOROOT/.nuget
export DOTNET_INSTALL_DIR=$REPOROOT/.dotnet
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
mkdir $DOTNET_INSTALL_DIR
curl -sSL https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.sh | bash /dev/stdin --install-dir $DOTNET_INSTALL_DIR

PATH="$DOTNET_INSTALL_DIR:$PATH"

dotnet restore CommandLine-netcore.sln 
dotnet publish ./dotnet/dotnet-netcore.csproj -r osx.10.11-x64 -f netcoreapp1.0
chmod +x ./dotnet/bin/Debug/netcoreapp1.0/osx. 10.11-x64/publish/dotnet