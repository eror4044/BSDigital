<script setup lang="ts">
import { ref, watch, onMounted } from "vue";
import VChart from "vue-echarts";
import type { EChartsType } from "echarts/core";

const props = defineProps<{ bids: [number, number][], asks: [number, number][] }>();

const chartRef = ref<InstanceType<typeof VChart> | null>(null);
let chart: EChartsType | null = null;

function updateChart() {
    if (!chart || !props.bids.length || !props.asks.length) return;

    chart.setOption({
        tooltip: { trigger: "axis" },
        grid: { left: 60, right: 30, top: 40, bottom: 50 },
        xAxis: { type: "value", name: "Price (EUR)", nameLocation: "middle", nameGap: 35 },
        yAxis: { type: "value", name: "Cumulative BTC", nameLocation: "middle", nameGap: 45 },
        series: [
            { name: "Bids", type: "line", step: "end", data: props.bids, lineStyle: { color: "#d62728" }, areaStyle: { color: "rgba(214,39,40,0.4)" } },
            { name: "Asks", type: "line", step: "end", data: props.asks, lineStyle: { color: "#2ca02c" }, areaStyle: { color: "rgba(44,160,44,0.4)" } }
        ]
    });
}

watch(() => [props.bids, props.asks], updateChart, { deep: true });

onMounted(() => {
    if (chartRef.value?.chart) chart = chartRef.value.chart;
    updateChart();
});
</script>

<template>
    <v-chart ref="chartRef" class="chart" :option="{}" autoresize />
</template>

<style scoped>
.chart {
    width: 100%;
    height: 500px;
}
</style>
