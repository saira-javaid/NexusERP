import { Injectable, inject } from '@angular/core';
import * as XLSX from 'xlsx';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

@Injectable({ providedIn: 'root' })
export class ExportService {
  exportToExcel<T extends Record<string, unknown>>(data: T[], filename: string, sheetName = 'Sheet1'): void {
    const worksheet = XLSX.utils.json_to_sheet(data);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, sheetName);
    XLSX.writeFile(workbook, `${filename}.xlsx`);
  }

  exportToPdf<T extends Record<string, unknown>>(
    data: T[], columns: { header: string; key: keyof T }[], filename: string, title?: string
  ): void {
    const doc = new jsPDF();
    if (title) {
      doc.setFontSize(16);
      doc.text(title, 14, 20);
    }
    autoTable(doc, {
      startY: title ? 30 : 20,
      head: [columns.map(c => c.header)],
      body: data.map(row => columns.map(c => String(row[c.key] ?? ''))),
    });
    doc.save(`${filename}.pdf`);
  }
}
