# kafka-dotnet-docker-demo

## 1. Run kafka
```
docker-compose up zookeeper broker
```

## 2. Run MoneyLaunderingService
```
docker-compose up --build money-laundering-service
```

## 3. Run TransactionAnalyticsService
```
docker-compose up --build transaction-analytics-service
```

## 4. Run CoreBankingSystem
### For a 'ok' transaction
```
docker-compose up --build core-banking-system-ok
```

### For a 'declined' transaction
```
docker-compose up --build core-banking-system-declined
```

## Important
The first transaction sent with the ```core-banking-system``` gets ignored. However, the second one should be received by the system. Some issue with my docker setup, it works flawlessly on localhost.
