### PoC of the distributed request telemetry

## Dependencies

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