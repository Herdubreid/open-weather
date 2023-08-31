import 'bootstrap';
import Plotly from 'plotly.js-dist';

window.plot = {
    purge: (div) => {
        try {
            Plotly.purge(div);
        } catch {}
    },
    animate: (div, data, frames, attribs) => {
        //console.log('Animate', data);
        try {
            Plotly.animate(div, data, frames, attribs);
        } catch {}
    },
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
        try {
            Plotly.newPlot(div, data, layout, { staticPlot: true });
        } catch {}
    },
    line: (div, data, layout, options, update) => {
        try {
            if (update) {
                //console.log('Update', div);
                Plotly.react(div, data, layout, options);
            }
            else {
                //console.log('Draw', div, data);
                Plotly.newPlot(div, data, layout, options);
            }
        } catch {}
    },
    relayout: (div, layout) => {
        try {
            Plotly.relayout(div, layout);
        } catch {}
    }
}
