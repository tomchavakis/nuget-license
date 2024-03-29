FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /src


COPY src/NugetUtility.csproj ./
RUN dotnet restore


COPY . ./
RUN dotnet publish -f net7.0 -c Release -o out


FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /src
COPY --from=build-env /src/out .
ENTRYPOINT ["dotnet", "NugetUtility.dll"]
