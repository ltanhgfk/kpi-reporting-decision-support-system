//window.renderPieChart = function (labels, values, canvasId, title, colors) {
//    const ctx = document.getElementById(canvasId).getContext('2d');
//    const data = {
//        labels: labels,
//        datasets: [{
//            data: values,
//            backgroundColor: colors || ['#36A2EB', '#FF6384'],
//            hoverOffset: 20
//        }]
//    };

//    // Calculate total for use in multiple places
//    const total = values.reduce((a, b) => a + b, 0);

//    // Helper function to format numbers with thousand separators
//    const formatNumber = (num) => {
//        return num.toLocaleString('vi-VN');
//    };

//    new Chart(ctx, {
//        type: 'pie',
//        data: data,
//        options: {
//            responsive: true,
//            plugins: {
//                title: {
//                    display: true,
//                    text: title,
//                    position: 'bottom',
//                    font: {
//                        size: 14
//                    }
//                },
//                legend: {
//                    position: 'top',
//                },
//                tooltip: {
//                    callbacks: {
//                        label: function (context) {
//                            let label = context.label || '';
//                            let value = context.parsed || 0;
//                            let percentage = ((value / total) * 100).toFixed(2);
//                            return `${label}: ${formatNumber(value)} (${percentage}%)`;
//                        }
//                    }
//                },
//                datalabels: {
//                    formatter: (value, ctx) => {
//                        let percentage = ((value / total) * 100).toFixed(2);
//                        return `${formatNumber(value)}\n(${percentage}%)`;
//                    },
//                    color: '#fff',
//                    font: {
//                        size: 11,
//                        weight: 'bold'
//                    }
//                }
//            }
//        },
//        plugins: [ChartDataLabels, {
//            // Custom plugin to display total in center
//            id: 'centerTotal',
//            afterDraw: (chart) => {
//                const ctx = chart.ctx;
//                ctx.save();
//                const centerX = (chart.chartArea.left + chart.chartArea.right) / 2;
//                const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;

//                ctx.textAlign = 'center';
//                ctx.textBaseline = 'middle';
//                ctx.font = 'bold 16px Arial';
//                ctx.fillStyle = '#fff';
//                ctx.fillText(`Tổng: ${formatNumber(total)}`, centerX, centerY);
//                ctx.restore();
//            }
//        }]
//    });
//};

window.renderPieChart = function (labels, values, canvasId, title, colors) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    const data = {
        labels: labels,
        datasets: [{
            data: values,
            backgroundColor: colors || ['#36A2EB', '#FF6384'],
            hoverOffset: 20
        }]
    };
    // Calculate total for use in multiple places
    const total = values.reduce((a, b) => a + b, 0);
    // Helper function to format numbers with thousand separators
    const formatNumber = (num) => {
        return num.toLocaleString('vi-VN');
    };
    new Chart(ctx, {
        type: 'pie',
        data: data,
        options: {
            responsive: true,
            layout: {
                padding: {
                    left: 40, // Tăng padding bên trái để có chỗ cho text đơn vị
                    right: 10,
                    top: 10,
                    bottom: 10
                }
            },
            plugins: {
                title: {
                    display: true,
                    text: title,
                    position: 'bottom',
                    font: {
                        size: 14
                    }
                },
                legend: {
                    position: 'top',
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            let label = context.label || '';
                            let value = context.parsed || 0;
                            let percentage = ((value / total) * 100).toFixed(2);
                            return `${label}: ${formatNumber(value)} (${percentage}%)`;
                        }
                    }
                },
                datalabels: {
                    formatter: (value, ctx) => {
                        let percentage = ((value / total) * 100).toFixed(2);
                        return `${formatNumber(value)}\n(${percentage}%)`;
                    },
                    color: '#fff',
                    font: {
                        size: 11,
                        weight: 'bold'
                    }
                }
            }
        },
        plugins: [ChartDataLabels, {
            // Custom plugin to display total in center
            id: 'centerTotal',
            afterDraw: (chart) => {
                const ctx = chart.ctx;
                ctx.save();
                const centerX = (chart.chartArea.left + chart.chartArea.right) / 2;
                const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.font = 'bold 16px Arial';
                ctx.fillStyle = '#fff';
                ctx.fillText(`${formatNumber(total)}`, centerX, centerY);//'Tổng ${formatNumber(total)
                ctx.restore();
            }
        }, {
                // Custom plugin to display unit text vertically on the left
                id: 'unitDisplay',
                afterDraw: (chart) => {
                    const ctx = chart.ctx;
                    ctx.save();

                    // Vị trí bên trái biểu đồ
                    const leftX = 15;
                    const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;

                    // Xoay canvas để viết text theo chiều dọc
                    ctx.translate(leftX, centerY);
                    ctx.rotate(-Math.PI / 2);

                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';
                    ctx.font = '12px Arial';
                    ctx.fillStyle = '#666';
                    ctx.fillText('(Đơn vị tính: triệu đồng)', 0, 0);

                    ctx.restore();
                }
            }]
    });
};

// chart-pie.js
//window.renderPieChart = function (labels, values, canvasId, title, colors) {
//    const ctx = document.getElementById(canvasId).getContext('2d');//'pieChartTongThu'
//    const data = {
//        labels: labels,
//        datasets: [{
//            data: values,
//            backgroundColor: [colors[0], colors[1]], /*['#36A2EB', '#FF6384'],*/
//            hoverOffset: 20
//        }]
//    };

//    new Chart(ctx, {
//        type: 'pie',
//        data: data,
//        options: {
//            responsive: true,
//            //cutout: '50%',
//            plugins: {
//                title: {
//                    display: true,
//                    text: title,
//                    position: 'bottom',
//                    font: {
//                        size: 14
//                    }
//                },

//                legend: {
//                    position: 'top',
//                    //position: 'left',
//                    //align: 'start', // hoặc
//                },
//                tooltip: {
//                    callbacks: {
//                        label: function (context) {
//                            let label = context.label || '';
//                            let value = context.parsed || 0;
//                            let total = context.dataset.data.reduce((a, b) => a + b, 0);
//                            let percentage = ((parseInt(value) / parseInt(total)) * 100).toFixed(2);
//                            return `${label}: ${value} (${percentage}%)`;
//                        }
//                    }
//                },
//                datalabels: {
//                    formatter: (value, ctx) => {
//                        let total = ctx.dataset.data.reduce((a, b) => a + b, 0);
//                        let percentage = ((parseInt(value) / parseInt(total)) * 100).toFixed(2);
//                        return `${value}\n(${percentage}%)`;
//                    },
//                    color: '#fff',
//                    font: {
//                        size: 11,
//                        weight: 'bold',
//                    }
//                }
//            }
//        },
//        plugins: [ChartDataLabels]
//    });
//};

//window.renderPieChart = function (labels, values, canvasId, title, colors) {
//    const ctx = document.getElementById(canvasId).getContext('2d');
//    const data = {
//        labels: labels,
//        datasets: [{
//            data: values,
//            backgroundColor: colors || ['#36A2EB', '#FF6384'],
//            hoverOffset: 20
//        }]
//    };

//    // Calculate total for use in multiple places
//    const total = values.reduce((a, b) => a + b, 0);
//    // Helper function to format numbers with thousand separators
//    const formatNumber = (num) => {
//        return num.toLocaleString('en-US');
//    };

//    new Chart(ctx, {
//        type: 'pie',
//        data: data,
//        options: {
//            responsive: true,
//            plugins: {
//                title: {
//                    display: true,
//                    text: title,
//                    position: 'top',
//                    font: {
//                        size: 16
//                    }
//                },
//                legend: {
//                    position: 'bottom',
//                },
//                tooltip: {
//                    callbacks: {
//                        label: function (context) {
//                            let label = context.label || '';
//                            let value = context.parsed || 0;
//                            let percentage = ((value / total) * 100).toFixed(2);
//                            return `${label}: ${value} (${percentage}%)`;
//                        }
//                    }
//                },
//                datalabels: {
//                    formatter: (value, ctx) => {
//                        let percentage = ((value / total) * 100).toFixed(2);
//                        return `${value}\n(${percentage}%)`;
//                    },
//                    color: '#fff',
//                    font: {
//                        size: 11,
//                        weight: 'bold'
//                    }
//                }
//            }
//        },
//        plugins: [ChartDataLabels, {
//            // Custom plugin to display total in center
//            id: 'centerTotal',
//            afterDraw: (chart) => {
//                const ctx = chart.ctx;
//                ctx.save();
//                const centerX = (chart.chartArea.left + chart.chartArea.right) / 5;
//                const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2; /*Tọa độ chữ*/

//                ctx.textAlign = 'top';
//                ctx.textBaseline = 'top';
//                ctx.font = 'bold 16px Arial';
//                ctx.fillStyle = '#000';
//                ctx.fillText(`Tổng: ${total}`, centerX, centerY);
//                ctx.restore();
//            }
//        }]
//    });
//};

window.renderDonutChart = function (labels, values, canvasId, title, colors) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    const data = {
        labels: labels,
        datasets: [{
            data: values,
            backgroundColor: [colors[0], colors[1]],
            hoverOffset: 20
        }]
    };

    // Calculate total for use in multiple places
    const total = values.reduce((a, b) => a + b, 0);

    // Helper function to format numbers with thousand separators
    const formatNumber = (num) => {
        return num.toLocaleString('vi-VN');
    };

    new Chart(ctx, {
        type: 'doughnut',
        data: data,
        options: {
            responsive: true,
            cutout: '40%',
            layout: {
                padding: {
                    left: 40, // Tăng padding bên trái để có chỗ cho text đơn vị
                    right: 10,
                    top: 10,
                    bottom: 10
                }
            },
            plugins: {
                title: {
                    display: true,
                    text: title,
                    position: 'bottom',
                    font: {
                        size: 14
                    }
                },
                legend: {
                    position: 'top',
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            let label = context.label || '';
                            let value = context.parsed || 0;
                            let percentage = ((parseInt(value) / parseInt(total)) * 100).toFixed(2);
                            return `${label}: ${formatNumber(value)} (${percentage}%)`;
                        }
                    }
                },
                datalabels: {
                    formatter: (value, ctx) => {
                        let percentage = ((parseInt(value) / parseInt(total)) * 100).toFixed(2);
                        return `${formatNumber(value)}\n(${percentage}%)`;
                    },
                    color: '#fff',
                    font: {
                        size: 11,
                        weight: 'bold'
                    }
                }
            }
        },
        plugins: [ChartDataLabels, {
            // Custom plugin to display total in center
            id: 'centerTotal',
            afterDraw: (chart) => {
                const ctx = chart.ctx;
                ctx.save();
                const centerX = (chart.chartArea.left + chart.chartArea.right) / 2;
                const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.font = 'bold 16px Arial';
                ctx.fillStyle = '#333';
                ctx.fillText(`${formatNumber(total)}`, centerX, centerY);////'Tổng ${formatNumber(total)
                ctx.restore();
            }
        }, {
                // Custom plugin to display unit text vertically on the left
                id: 'unitDisplay',
                afterDraw: (chart) => {
                    const ctx = chart.ctx;
                    ctx.save();

                    // Vị trí bên trái biểu đồ
                    const leftX = 15;
                    const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;

                    // Xoay canvas để viết text theo chiều dọc
                    ctx.translate(leftX, centerY);
                    ctx.rotate(-Math.PI / 2);

                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';
                    ctx.font = '12px Arial';
                    ctx.fillStyle = '#666';
                    ctx.fillText('(Đơn vị tính: triệu đồng)', 0, 0);

                    ctx.restore();
                }
            }]
    });
};

//window.renderDonutChart = function (labels, values, canvasId, title, colors) {
//    const ctx = document.getElementById(canvasId).getContext('2d');//'pieChartTongThu'
//    const data = {
//        labels: labels,
//        datasets: [{
//            data: values,
//            backgroundColor: [colors[0], colors[1]],/*['#888', '#FF6384'], *//*colors,/*[[colors[0], colors[1]],*/ /*['#888', '#FF6384'],*/
//            hoverOffset: 20
//        }]
//    };    

//    new Chart(ctx, {
//        type: 'doughnut',
//        data: data,
//        options: {
//            responsive: true,
//            cutout: '40%',            
//            plugins: {
//                title: {
//                    display: true,
//                    text: title,
//                    position: 'bottom',
//                    font: {
//                        size: 14
//                    }
//                },
//                legend: {
//                    position: 'top',
//                    //position: 'left',
//                    //align: 'start',
//                },
//                tooltip: {
//                    callbacks: {
//                        label: function (context) {
//                            let label = context.label || '';
//                            let value = context.parsed || 0;
//                            let total = context.dataset.data.reduce((a, b) => a + b, 0);
//                            let percentage = ((parseInt(value) / parseInt(total)) * 100).toFixed(2);
//                            return `${label}: ${value} (${percentage}%)`;
//                        }
//                    }
//                },
//                datalabels: {
//                    formatter: (value, ctx) => {
//                        let total = ctx.dataset.data.reduce((a, b) => a + b, 0);
//                        let percentage = ((parseInt(value) / parseInt(total)) * 100).toFixed(2);
//                        return `${value}\n(${percentage}%)`;
//                    },
//                    color: '#fff',
//                    font: {
//                        size: 11,
//                        weight: 'bold'
//                    }
//                }
//            }
//        },
//        plugins: [ChartDataLabels]
//        //plugins: [ChartDataLabels, {
//        //    // Custom plugin to display total in center
//        //    id: 'centerTotal',
//        //    afterDraw: (chart) => {
//        //        const ctx = chart.ctx;
//        //        ctx.save();
//        //        const centerX = (chart.chartArea.left + chart.chartArea.right) / 2;
//        //        const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;
//        //        ctx.textAlign = 'center';
//        //        ctx.textBaseline = 'middle';
//        //        ctx.font = 'bold 16px Arial';
//        //        ctx.fillStyle = '#fff';
//        //        ctx.fillText(`Tổng: ${formatNumber(total)}`, centerX, centerY);
//        //        ctx.restore();
//        //    }
//        //}, {
//        //        // Custom plugin to display unit text vertically on the left
//        //        id: 'unitDisplay',
//        //        afterDraw: (chart) => {
//        //            const ctx = chart.ctx;
//        //            ctx.save();

//        //            // Vị trí bên trái biểu đồ
//        //            const leftX = 15;
//        //            const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;

//        //            // Xoay canvas để viết text theo chiều dọc
//        //            ctx.translate(leftX, centerY);
//        //            ctx.rotate(-Math.PI / 2);

//        //            ctx.textAlign = 'center';
//        //            ctx.textBaseline = 'middle';
//        //            ctx.font = '12px Arial';
//        //            ctx.fillStyle = '#666';
//        //            ctx.fillText('(Đơn vị tính: triệu đồng)', 0, 0);

//        //            ctx.restore();
//        //        }
//        //    }]
//    });
//};

// chart-bar.js
window.renderBarChart = function (canvasId, labels, thuchien, kehoach, cungky, title, xTitle, yTitle, colors) {
    const ctx = document.getElementById(canvasId).getContext('2d');    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Thực hiện',
                    data: thuchien,
                    backgroundColor: 'rgba(75, 192, 192, 0.7)',
                    barPercentage: 0.5,
                    categoryPercentage: 0.5
                },     
                {
                    label: 'Kế hoạch',
                    data: kehoach,
                    backgroundColor: 'rgba(255, 205, 86, 0.7)',
                    barPercentage: 0.5,
                    categoryPercentage: 0.5
                },                           
                //{
                //    label: 'Cùng kỳ',
                //    data: cungky,
                //    backgroundColor: 'rgba(255, 99, 132, 0.7)',
                //    barPercentage: 0.5,
                //    categoryPercentage: 0.5
                //}
            ]
        },
        options: {
            responsive: true,            
            plugins: {
                title: {
                    display: true,
                    text: title,
                    font: {
                        size: 14
                    }
                },
                legend: {
                    position: 'top'
                },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                x: {
                    stacked: false,
                    ticks: {
                        maxRotation: 45,
                        minRotation: 0
                    },
                    grid: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: xTitle
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: '#e0e0e0'
                    },
                    title: {
                        display: true,
                        text: yTitle
                    }
                }
            }
        }
    });
};


//---------------------------------------------------------------------//
//window.renderBarChart = function (labels, values) {
//    const ctxCT = document.getElementById('chartChitieu').getContext('2d');
//    const chart1 = new Chart(ctxCT, {
//        type: 'bar',
//        data: {
//            labels: labels,
//            datasets: [
//                {
//                    label: 'Thực hiện',
//                    data: @Html.Raw(Json.Encode(thuchien)),
//                    backgroundColor: 'rgba(75, 192, 192, 0.7)',
//                    barPercentage: 0.5,
//                    categoryPercentage: 0.5
//                },
//                {
//                    label: 'Kế hoạch',
//                    data: @Html.Raw(Json.Encode(kehoach)),
//                    backgroundColor: 'rgba(255, 205, 86, 0.7)',
//                    barPercentage: 0.5,
//                    categoryPercentage: 0.5
//                },
//                {
//                    label: 'Cùng kỳ',
//                    data: @Html.Raw(Json.Encode(cungky)),
//                    backgroundColor: 'rgba(255, 99, 132, 0.7)',
//                    barPercentage: 0.5,
//                    categoryPercentage: 0.5
//                }
//            ]
//        },
//        options: {
//            responsive: true,
//            plugins: {
//                legend: {
//                    position: 'top'
//                },
//                tooltip: {
//                    mode: 'index',
//                    intersect: false
//                }
//            },
//            scales: {
//                x: {
//                    stacked: false,
//                    ticks: {
//                        maxRotation: 45,
//                        minRotation: 0
//                    },
//                    grid: {
//                        display: false
//                    }
//                },
//                y: {
//                    beginAtZero: true,
//                    grid: {
//                        color: '#e0e0e0'
//                    }
//                }
//            }
//        }
//    });
//};

//window.renderChartThang = function (labels, values) {
//    const ctxThang = document.getElementById('chartThang').getContext('2d');
//    const chart3 = new Chart(ctxThang, {
//        type: 'bar',
//        data: {
//            labels: @Html.Raw(Json.Encode(labelThang)),
//            datasets: [
//                {
//                    label: 'Thực hiện',
//                    data: @Html.Raw(Json.Encode(thuchienThang)),
//                    backgroundColor: 'rgba(75, 192, 192, 0.7)'
//                },
//                {
//                    label: 'Kế hoạch',
//                    data: @Html.Raw(Json.Encode(kehoachThang)),
//                    backgroundColor: 'rgba(255, 205, 86, 0.7)'
//                },
//                {
//                    label: 'Cùng kỳ',
//                    data: @Html.Raw(Json.Encode(cungkyThang)),
//                    backgroundColor: 'rgba(255, 99, 132, 0.7)'
//                }
//            ]
//        },
//        options: {
//            responsive: true,
//            scales: {
//                y: { beginAtZero: true }
//            }
//        }
//    });
//};

window.renderLineChart = function (canvasId, labels, thuchien, kehoach, cungky, title, xTitle, yTitle, colors) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    const chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Thực hiện',
                    data: thuchien,
                    borderColor: '#0000FF', /*'rgba(75, 192, 192, 1)'*/
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    borderWidth: 0.2,
                    pointRadius: 3,
                    pointBackgroundColor: 'rgba(75, 192, 192, 1)',
                    tension: 0.3,
                    fill: false
                },  
                {
                    label: 'Kế hoạch',
                    data: kehoach,
                    borderColor: '#FF00FF', /*'rgba(255, 205, 86, 1)',*/
                    backgroundColor: 'rgba(255, 205, 86, 0.2)',
                    borderWidth: 0.2,
                    pointRadius: 3,
                    pointBackgroundColor: 'rgba(255, 205, 86, 1)',
                    tension: 0.3,
                    fill: false
                },                              
                //{
                //    label: 'Cùng kỳ',
                //    data: cungky,
                //    borderColor: '#006600',/*'rgba(255, 99, 132, 1)',*/
                //    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                //    borderWidth: 0.2,
                //    pointRadius: 3,
                //    pointBackgroundColor: 'rgba(255, 99, 132, 1)',
                //    tension: 0.3,
                //    fill: false
                //}
            ]
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: title,
                    font: {
                        size: 14
                    }
                },
                legend: {
                    position: 'top',
                    labels: {
                        boxWidth: 12,
                        padding: 20
                    }
                },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                x: {
                    ticks: {
                        maxRotation: 45,
                        autoSkip: true,
                        maxTicksLimit: 10
                    },
                    grid: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: xTitle
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        drawBorder: false,
                        color: 'rgba(200,200,200,0.2)',
                        borderDash: [5, 5]
                    },
                    ticks: {
                        stepSize: Math.ceil(Math.max(thuchien) / 5)
                    },
                    title: {
                        display: true,
                        text: yTitle
                    }
                }
            }
        }
    });
};

