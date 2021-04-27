FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /src


COPY src/NugetUtility.csproj ./
RUN dotnet restore


COPY . ./
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/runtime:3.1
WORKDIR /src
COPY --from=build-env /src/out .
ENTRYPOINT ["dotnet", "NugetUtility.dll"]