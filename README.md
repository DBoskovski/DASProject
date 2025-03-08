Kafka фолдерот внатре во него во во 2 различни cmd-а ги пуштаме наредните команди
1. .\bin\windows\zookeeper-server-start.bat .\config\zookeeper.properties
за активација на zookeeper
2. .\bin\windows\kafka-server-start.bat .\config\server.properties
за активација на kafka

потоа внатре во \bin\windows kafka фолдерот ги пуштаме командите
1.kafka-topics.bat --create --bootstrap-server localhost:9092 --topic stockprices
за креирање на topic stockprices
2.kafka-topics.bat --create --bootstrap-server localhost:9092 --topic processedstockprices
за креирање на topic processedstockprices
3.kafka-console-producer.bat --bootstrap-server localhost:9092 --topic stockprices 
за пуштање на stockprices како producer
4.kafka-console-consumer.bat --topic processedstockprices --bootstrap-server localhost:9092 --from-beginning
за пуштање на processedstockprices како consumer

