// Dashboard Charts Initializer using Chart.js
function initDashboardCharts(byCategoryData, byStatusData, byLocationData, byDepartmentData) {
    // 1. Assets by Category Chart (Doughnut)
    const ctxCategory = document.getElementById('chartByCategory');
    if (ctxCategory && byCategoryData) {
        new Chart(ctxCategory, {
            type: 'doughnut',
            data: {
                labels: Object.keys(byCategoryData),
                datasets: [{
                    data: Object.values(byCategoryData),
                    backgroundColor: [
                        '#4f46e5', '#10b981', '#f59e0b', '#ef4444',
                        '#06b6d4', '#8b5cf6', '#ec4899', '#64748b'
                    ],
                    borderWidth: 2,
                    borderColor: '#ffffff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'right' }
                }
            }
        });
    }

    // 2. Assets by Status Chart (Bar)
    const ctxStatus = document.getElementById('chartByStatus');
    if (ctxStatus && byStatusData) {
        new Chart(ctxStatus, {
            type: 'bar',
            data: {
                labels: Object.keys(byStatusData),
                datasets: [{
                    label: 'Asset Count',
                    data: Object.values(byStatusData),
                    backgroundColor: [
                        '#10b981', '#3b82f6', '#f59e0b', '#ef4444', '#64748b', '#1f2937'
                    ],
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, ticks: { precision: 0 } }
                }
            }
        });
    }

    // 3. Assets by Location Chart (Pie)
    const ctxLocation = document.getElementById('chartByLocation');
    if (ctxLocation && byLocationData) {
        new Chart(ctxLocation, {
            type: 'pie',
            data: {
                labels: Object.keys(byLocationData),
                datasets: [{
                    data: Object.values(byLocationData),
                    backgroundColor: [
                        '#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#ec4899', '#06b6d4'
                    ]
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: 'bottom' } }
            }
        });
    }

    // 4. Assets by Department Chart (Horizontal Bar)
    const ctxDepartment = document.getElementById('chartByDepartment');
    if (ctxDepartment && byDepartmentData) {
        new Chart(ctxDepartment, {
            type: 'bar',
            data: {
                labels: Object.keys(byDepartmentData),
                datasets: [{
                    label: 'Assets',
                    data: Object.values(byDepartmentData),
                    backgroundColor: '#8b5cf6',
                    borderRadius: 6
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: { beginAtZero: true, ticks: { precision: 0 } }
                }
            }
        });
    }
}
