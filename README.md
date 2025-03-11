# StockDashboard

## Overview
StockDashboard is an API that provides stock market data. It fetches data from external APIs, caches it using Redis, and secures sensitive information using Azure Key Vault.

## Project Structure
.github/
    workflows/
.gitignore
.vs/
    
appsettings.Development.json
appsettings.json
bin/
    Debug/
    Release/
Controllers/
    StockController.cs
Middleware/
    
Models/
    
    
    StockResponse.cs
obj/
    Debug/

Properties/
Repositories/
    
StockDashBoard.API.csproj
StockDashBoard.API.sln
terraform/


## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Azure account for Key Vault
- Redis server

### Configuration
1. **Azure Key Vault**: Store your API keys and other secrets in Azure Key Vault.
2. **Redis**: Configure your Redis connection string in `appsettings.json`.

### Installation
1. Clone the repository:
    ```sh
    git clone https://github.com/ThembaFumani/StockDashboard.API.git
    cd StockDashboard/StockDashBoard.API
    ```

2. Restore the dependencies:
    ```sh
    dotnet restore
    ```

3. Build the project:
    ```sh
    dotnet build
    ```

### Running the Application
1. Update the `appsettings.Development.json` with your configuration.
2. Run the application:
    ```sh
    dotnet run
    ```

### API Endpoints
- **GET /api/stocks/{symbol}**: Fetches stock data for the given symbol.

### Middleware
- **ApiKeyMiddleware**: Validates the API key provided in the request headers.

### Models
- **StockData**: Represents the stock data.
- **StockMetaData**: Represents the metadata of the stock.
- **StockResponse**: Represents the response from the stock API.

### Repositories
- **StockRepository**: Handles fetching and caching of stock data.

## Contributing
1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Commit your changes (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a new Pull Request.

## License
This project is licensed under the MIT License.