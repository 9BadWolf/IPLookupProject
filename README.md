# IPLookup Project


To run the project, the provided `docker-compose.yml` file is used, which handles the compilation (`docker compose build`) as well as the creation and execution of the containers (`docker compose up`).

The following environment variables are required for proper execution:

**IpLookup container**

-   `IpStack__AccessKey` -> API key for IpStack

**CachingApi container**

-   `LookupApi__BaseUrl`

**BatchProcessor**

-   `CachingApi__BaseUrl`

The two `BaseUrl` values should be in the format `https://<containername>:<port>`, where you need to specify the `containername` and `port` of the respective microservices with values that match the execution environment.
