export interface OrderBookCellProps {
  side: "bid" | "ask";
  amount: number | null;
  max: number;
}

export interface OrderBookChartProps {
  bids: [number, number][];
  asks: [number, number][];
}