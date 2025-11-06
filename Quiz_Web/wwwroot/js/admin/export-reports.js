// Common export functions for all reports
function exportToCSV(reportName) {
    const tables = document.querySelectorAll('table');
    let csvContent = reportName + '\n\n';
    
    tables.forEach((table) => {
        const title = table.closest('.card').querySelector('.card-header h5')?.textContent || 'Data';
        csvContent += title + '\n';
        
        const rows = table.querySelectorAll('tr');
        rows.forEach(row => {
            const cols = row.querySelectorAll('th, td');
            const rowData = Array.from(cols).map(col => col.textContent.trim()).join(',');
            csvContent += rowData + '\n';
        });
        csvContent += '\n';
    });
    
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = reportName.toLowerCase().replace(/\s+/g, '_') + '_' + new Date().toISOString().split('T')[0] + '.csv';
    link.click();
}

function exportToExcel(reportName) {
    const wb = XLSX.utils.book_new();
    const tables = document.querySelectorAll('table');
    
    tables.forEach((table, index) => {
        const title = table.closest('.card').querySelector('.card-header h5')?.textContent || `Sheet${index + 1}`;
        const ws = XLSX.utils.table_to_sheet(table);
        XLSX.utils.book_append_sheet(wb, ws, title.substring(0, 31));
    });
    
    XLSX.writeFile(wb, reportName.toLowerCase().replace(/\s+/g, '_') + '_' + new Date().toISOString().split('T')[0] + '.xlsx');
}