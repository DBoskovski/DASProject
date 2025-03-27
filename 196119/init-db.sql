CREATE TABLE processed_stock_prices (
    symbol VARCHAR(10) NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    close_price DECIMAL NOT NULL,
    moving_average DECIMAL NOT NULL,
    trend VARCHAR(20) NOT NULL,
    PRIMARY KEY (symbol, timestamp)
);