export interface OrderLevel {
  Price: number;
  Amount: number;
}

export interface OrderBookSnapshot {
  timestamp: string;
  bids: string;
  asks: string;
}

export interface ParsedOrderBookSnapshot {
  timestamp: string;
  bids: OrderLevel[];
  asks: OrderLevel[];
}

export interface QuoteResult {
  requested: number;
  filled: number;
  totalCost: number;
  averagePrice: number;
  sufficient: boolean;
  timestampUtc: string;
}
