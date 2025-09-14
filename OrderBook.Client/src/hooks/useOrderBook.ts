import { ref, watch } from "vue";
import * as signalR from "@microsoft/signalr";

const QUOTE_DEBOUNCE_MS = 400;
const LEVELS_LIMIT = 50;

export function useOrderBook() {
    const bids = ref<[number, number][]>([]);
    const asks = ref<[number, number][]>([]);
    const status = ref<"disconnected" | "connected" | "failed">("disconnected");
    const errorMessage = ref<string | null>(null);
    const amountBtc = ref<number>(0.1);
    const quote = ref<{ total: number; avg: number; filled: number; ok: boolean; ts?: string } | null>(null);

    let quoteTimer: number | undefined;

    async function fetchQuote() {
        try {
            const resp = await fetch(
                `http://localhost:5000/api/quotes?amount=${amountBtc.value}&type=buy`,
                { credentials: "omit" }
            );
            const data = await resp.json();
            quote.value = {
                total: data.totalCost,
                avg: data.averagePrice,
                filled: data.filled,
                ok: data.sufficient,
                ts: data.timestampUtc
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
            .withUrl("http://localhost:5000/hubs/orderbook", { transport: signalR.HttpTransportType.WebSockets })
            .withAutomaticReconnect()
            .build();

        connection.on("orderbook:update", (data: { bids: [string, string][], asks: [string, string][] }) => {
            if (!data?.bids || !data?.asks) return;

            const parsedBids = data.bids.slice(0, LEVELS_LIMIT).map(([p, v]) => [parseFloat(p), parseFloat(v)] as [number, number]).sort((a, b) => b[0] - a[0]);
            const parsedAsks = data.asks.slice(0, LEVELS_LIMIT).map(([p, v]) => [parseFloat(p), parseFloat(v)] as [number, number]).sort((a, b) => a[0] - b[0]);

            let cum = 0;
            bids.value = parsedBids.map(([p, v]) => [p, (cum += v)]);
            cum = 0;
            asks.value = parsedAsks.map(([p, v]) => [p, (cum += v)]);
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
