import { use } from "echarts/core";
import { CanvasRenderer } from "echarts/renderers";
import { LineChart } from "echarts/charts";
import {
    GridComponent,
    TooltipComponent,
    TitleComponent,
    LegendComponent
} from "echarts/components";

use([
    CanvasRenderer,
    LineChart,
    GridComponent,
    TooltipComponent,
    TitleComponent,
    LegendComponent
]);
