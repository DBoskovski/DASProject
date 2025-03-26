1. docker-compose up --build					- за да се направат сликите за DOCKER, јас работев со DOCKER DESKTOP.


2. docker-compose exec db psql -U postgres -d proekt

CREATE TABLE processed_stock_prices (
    symbol VARCHAR(10) NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    close_price DECIMAL NOT NULL,
    moving_average DECIMAL NOT NULL,
    trend VARCHAR(20) NOT NULL,
    PRIMARY KEY (symbol, timestamp)
);
\q								- првиот пат има потреба од креирање на табелата затоа што во сликата ја нема табелата и потоа се е 
								  како што и во самата слика си се чуваат сите податоци.



