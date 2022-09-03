FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /src


COPY src/NugetUtility.csproj ./
RUN dotnet restore


COPY . ./
RUN dotnet publish -f net6.0 -c Release -o out


FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /src
COPY --from=build-env /src/out .
ENTRYPOINT ["dotnet", "NugetUtility.dll"]