import { ref, watch } from "vue";
import * as signalR from "@microsoft/signalr";
import type { QuoteResult, OrderLevel } from "../shared/types/orderBook";

const QUOTE_DEBOUNCE_MS = 400;
const LEVELS_LIMIT = 50;
const API_URL = import.meta.env.VITE_API_URL;

export function useOrderBook() {
  const bids = ref<OrderLevel[]>([]);
  const asks = ref<OrderLevel[]>([]);

  const status = ref<"disconnected" | "connected" | "failed">("disconnected");
  const errorMessage = ref<string | null>(null);

  const amountBtc = ref<number>(0.1);

  const quote = ref<QuoteResult | null>(null);

  let quoteTimer: number | undefined;

  async function fetchQuote() {
    try {
      const resp = await fetch(
        `${API_URL}/api/quotes?amount=${amountBtc.value}&type=buy`,
        { credentials: "omit" }
      );
      const data = await resp.json();
      quote.value = {
        requested: amountBtc.value,
        filled: data.filled,
        totalCost: data.totalCost,
        averagePrice: data.averagePrice,
        sufficient: data.sufficient,
        timestampUtc: data.timestampUtc
      };
    } catch (e) {
      console.error("quote error", e);
    }
  }

  watch(amountBtc, (newVal, oldVal) => {
    if (newVal === oldVal) return;
    if (quoteTimer) window.clearTimeout(quoteTimer);
    quoteTimer = window.setTimeout(fetchQuote, QUOTE_DEBOUNCE_MS);
  });

  async function connect() {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/orderbook`, {
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    connection.on("orderbook:update", (data: { bids: [string, string][], asks: [string, string][] }) => {
      if (!data?.bids || !data?.asks) return;

      const parsedBids: OrderLevel[] = data.bids
        .slice(0, LEVELS_LIMIT)
        .map(([p, v]) => ({ Price: parseFloat(p), Amount: parseFloat(v) }))
        .sort((a, b) => b.Price - a.Price);

      const parsedAsks: OrderLevel[] = data.asks
        .slice(0, LEVELS_LIMIT)
        .map(([p, v]) => ({ Price: parseFloat(p), Amount: parseFloat(v) }))
        .sort((a, b) => a.Price - b.Price);

      let cum = 0;
      bids.value = parsedBids.map(l => ({ ...l, Amount: (cum += l.Amount) }));
      cum = 0;
      asks.value = parsedAsks.map(l => ({ ...l, Amount: (cum += l.Amount) }));
    });

    try {
      await connection.start();
      status.value = "connected";
      fetchQuote();
    } catch (err) {
      console.error("SignalR connection error:", err);
      status.value = "failed";
      errorMessage.value = (err as Error).message;
    }
  }

  return { bids, asks, status, errorMessage, amountBtc, quote, fetchQuote, connect };
}
