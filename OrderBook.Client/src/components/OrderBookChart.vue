<template>
  <div ref="chartRef" class="chart"></div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from "vue";
import * as echarts from "echarts";
import type { OrderLevel } from "@/shared/types/orderBook";

interface Props {
  bids: OrderLevel[];
  asks: OrderLevel[];
}

const props = defineProps<Props>();
const chartRef = ref<HTMLDivElement | null>(null);
let chart: echarts.ECharts | null = null;

function prepareData(levels: OrderLevel[], isBid: boolean) {
  const sorted = [...levels].sort((a, b) =>
    isBid ? b.Price - a.Price : a.Price - b.Price
  );

  let cum = 0;
  return sorted.map(({ Price, Amount }) => {
    cum += Amount;
    return [Price, cum]; // echarts принимает массивы [x, y]
  });
}

function render() {
  if (!chart || !props.bids || !props.asks) return;

  const bidData = prepareData(props.bids, true);
  const askData = prepareData(props.asks, false);

  const allPrices = [...bidData.map(p => p[0]), ...askData.map(p => p[0])];
  const minPrice = Math.min(...allPrices);
  const maxPrice = Math.max(...allPrices);

  chart.setOption({
    tooltip: { trigger: "axis", axisPointer: { type: "cross" } },
    grid: { left: 35, right: 20, top: 40, bottom: 20 },
    xAxis: { type: "value", boundaryGap: false, min: minPrice, max: maxPrice },
    yAxis: { type: "value", boundaryGap: false },
    series: [
      {
        name: "Bids",
        type: "line",
        step: "end",
        symbol: "none",
        data: bidData,
        lineStyle: { color: "#0a0" },
        areaStyle: { color: "rgba(0,160,0,0.2)" }
      },
      {
        name: "Asks",
        type: "line",
        step: "end",
        symbol: "none",
        data: askData,
        lineStyle: { color: "#c00" },
        areaStyle: { color: "rgba(200,0,0,0.2)" }
      }
    ]
  });
}

onMounted(() => {
  if (chartRef.value) {
    chart = echarts.init(chartRef.value);
    render();
  }
});
onUnmounted(() => chart?.dispose());
watch(() => [props.bids, props.asks], render, { deep: true });
</script>

<style scoped>
.chart {
  width: 100%;
  height: 400px;
}
</style>
