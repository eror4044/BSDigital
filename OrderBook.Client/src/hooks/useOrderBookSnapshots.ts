import { ref } from "vue";
import type { OrderBookSnapshot } from "../shared/types/orderBook";

const API_URL = import.meta.env.VITE_API_URL;

export function useOrderBookSnapshots() {
  const snapshots = ref<OrderBookSnapshot[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchSnapshots(take = 20) {
    loading.value = true;
    try {
      const resp = await fetch(`${API_URL}/api/orderbook/snapshots?take=${take}`);
      const data = await resp.json();
      snapshots.value = data;
      error.value = null;
    } catch (e) {
      console.error("fetchSnapshots error", e);
      error.value = (e as Error).message;
    } finally {
      loading.value = false;
    }
  }

  return { snapshots, loading, error, fetchSnapshots };
}
