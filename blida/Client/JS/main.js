﻿import 'bootstrap';
import Plotly from 'plotly.js-dist';

window.plot = {
    draw: (div, title, value, avg, y, range) => {
        var data = [
            {
                type: "indicator",
                mode: "number+delta",
                value: value,
                delta: { reference: avg, valueformat: ".0f" },
                ticker: { showticker: true },
                vmax: 500,
                domain: { y: [0, 1], x: [0.25, 0.75] },
                title: { text: title }
            },
            {
                y: y
            }
        ];

        var layout = { width: 600, height: 450, xaxis: { range: range } };
        Plotly.newPlot(div, data, layout, { staticPlot: true });
    },
    line: (div, data, layout, options, update) => {
        //layout.width = document.getElementById(div).offsetWidth;
        if (update) {
            console.log('Update', div);
            Plotly.react(div, data, layout, options);
        }
        else {
            console.log('Draw', div);
            Plotly.newPlot(div, data, layout, options);
        }
    },
    relayout: (div, layout) => {
        Plotly.relayout(div, layout);
    }
}