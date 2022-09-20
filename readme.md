### PoC of the distributed request telemetry

## Dependencies

### .NET 6

You can install it from https://dotnet.microsoft.com/en-us/download

### Azure services

To increase the amount of data that OpenTelemetry can run collect and present, the repository uses a few Azure dependencies:

- ServiceBus - to send calculation requests from WebService to the Worker
- Table Storage - to store the result for given N
- SQL Database and Entity Framework - to store information about the caller of the POST /n endpoint. It doesnt bring any additional value and it was added only to generate extra traffic to external dependency.

You need to create these services and update configuration in the `appsettings.json` in both project, or create `appsettings.local.json` file with only these values to override empty strings.

### Zipkin

Project uses Zipking for presenting OpenTelemetry functionalities. You can use Docker to run it locally:

```bash
docker run -d -p 9411:9411 openzipkin/zipkin
```

Started instance will be available at http://localhost:9411/zipkin/

## Resources

1. https://opentelemetry.io/
2. https://github.com/open-telemetry/opentelemetry-dotnet
3. https://twitter.com/SergeyKanzhelev
4. https://w3c.github.io/correlation-context/
5. https://www.youtube.com/watch?v=FlghuHDlQdM
6. https://www.youtube.com/watch?v=p6CjlnwPhHQ