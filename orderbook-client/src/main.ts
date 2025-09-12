import { createApp } from 'vue'
import { createPinia } from 'pinia'
import VChart from "vue-echarts";

import App from './App.vue'
import router from './router'
import "./echarts";
const app = createApp(App)
app.component("v-chart", VChart);
app.use(createPinia())
app.use(router)

app.mount('#app')
