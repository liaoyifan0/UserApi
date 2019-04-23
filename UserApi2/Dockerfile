FROM microsoft/dotnet:sdk as build-env
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c release -o out

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /release
COPY --from=build-env /app/out ./

EXPOSE 80 5001
ENTRYPOINT ["dotnet","UserApi2.dll"]