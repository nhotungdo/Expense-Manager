/**
 * Transaction Utilities
 *
 * Provides input validation, DTO building, and API interaction helpers
 * for creating transactions (Income, Expense, Transfer).
 */

/**
 * Validate transaction input
 * @param {Object} input - raw form values
 * @param {number} input.type - 1=Income, 2=Expense, 3=Transfer
 * @param {number} input.amount - transaction amount
 * @param {string|number} input.accountId - source account id
 * @param {string} input.date - ISO date string (yyyy-MM-dd)
 * @param {string} [input.note] - optional note, max 512 chars
 * @param {string|number|null} [input.categoryId] - optional category id
 * @param {string|number|null} [input.pairedAccountId] - destination account id for transfers
 * @returns {{valid:boolean, errors:string[]}}
 */
export function validateTransactionInput(input) {
  const errors = [];

  const type = Number(input.type);
  if (![1, 2, 3].includes(type)) errors.push('Loại giao dịch không hợp lệ');

  const amount = Number(input.amount);
  if (!Number.isFinite(amount) || amount <= 0) errors.push('Số tiền phải lớn hơn 0');

  if (!input.accountId) errors.push('Vui lòng chọn tài khoản');

  if (!input.date) errors.push('Vui lòng chọn ngày thực hiện');

  const note = input.note || '';
  if (note.length > 512) errors.push('Ghi chú tối đa 512 ký tự');

  if (type === 3) {
    if (!input.pairedAccountId) errors.push('Vui lòng chọn tài khoản nhận');
    if (String(input.pairedAccountId) === String(input.accountId)) {
      errors.push('Không thể chuyển tiền đến cùng một ví');
    }
  }

  return { valid: errors.length === 0, errors };
}

/**
 * Build server DTO from valid input
 * @param {Object} input - same shape as validateTransactionInput
 * @returns {Object} dto payload for POST /api/Transactions
 */
export function buildTransactionDto(input) {
  const categoryId = input.categoryId ? Number(input.categoryId) : null;
  const pairedAccountId = input.pairedAccountId ? Number(input.pairedAccountId) : null;
  return {
    AccountId: Number(input.accountId),
    CategoryId: categoryId,
    TransactionType: Number(input.type),
    Amount: Number(input.amount),
    Currency: input.currency || 'VND',
    Note: input.note || '',
    TransactionDate: input.date,
    PairedAccountId: pairedAccountId,
    AttachmentUrl: input.attachmentUrl || ''
  };
}

/**
 * Call API to create transaction
 * @param {Object} dto - payload from buildTransactionDto
 * @param {AbortSignal} [signal] - optional abort signal to cancel request
 * @returns {Promise<{ok:boolean, status:number, data?:Object, error?:string}>}
 */
export async function createTransaction(dto, signal) {
  try {
    const res = await fetch('/api/Transactions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(dto),
      signal
    });

    const status = res.status;
    if (!res.ok) {
      let message = 'Không thể lưu giao dịch';
      try {
        const err = await res.json();
        message = err.message || message;
      } catch { /* ignore parse errors */ }
      return { ok: false, status, error: message };
    }

    const data = await res.json();
    return { ok: true, status, data };
  } catch (e) {
    // Network/abort errors
    return { ok: false, status: 0, error: e?.message || 'Lỗi kết nối' };
  }
}

/**
 * Call API to update transaction
 * @param {string|number} id - transaction id
 * @param {Object} dto - payload
 * @returns {Promise<{ok:boolean, status:number, data?:Object, error?:string}>}
 */
export async function updateTransaction(id, dto) {
  try {
    const res = await fetch(`/api/Transactions/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ ...dto, Id: id }) // ensure Id is in body
    });

    if (!res.ok) {
      let message = 'Không thể cập nhật giao dịch';
      try { const err = await res.json(); message = err.message || message; } catch { }
      return { ok: false, status: res.status, error: message };
    }
    const data = await res.json();
    return { ok: true, data };
  } catch (e) {
    return { ok: false, error: e.message };
  }
}

/**
 * Get transaction details
 * @param {string|number} id 
 * @returns {Promise<{ok:boolean, data?:Object, error?:string}>}
 */
export async function getTransaction(id) {
  try {
    const res = await fetch(`/api/Transactions/${id}`, {
      credentials: 'include',
      headers: {
        'Accept': 'application/json'
      }
    });
    if (!res.ok) return { ok: false };
    const data = await res.json();
    return { ok: true, data };
  } catch (e) {
    return { ok: false, error: e.message };
  }
}

/**
 * Delete transaction
 * @param {string|number} id 
 * @returns {Promise<{ok:boolean}>}
 */
export async function deleteTransaction(id) {
  try {
    const res = await fetch(`/api/Transactions/${id}`, {
      method: 'DELETE',
      credentials: 'include'
    });
    return { ok: res.ok };
  } catch (e) {
    return { ok: false };
  }
}

/**
 * Upload file attachment
 * @param {File} file 
 * @returns {Promise<{url: string}>}
 */
export async function uploadAttachment(file) {
  const formData = new FormData();
  formData.append('file', file);
  const res = await fetch('/api/Ocr/upload', {
    method: 'POST',
    credentials: 'include',
    body: formData
  });
  if (res.ok) return res.json();
  throw new Error('Upload failed');
}

/**
 * Process OCR
 * @param {string} base64Image 
 * @returns {Promise<{amount?:number, date?:string, vendor?:string}>}
 */
export async function processOcr(base64Image) {
  const res = await fetch('/api/Ocr/process', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify({ imageBase64: base64Image })
  });
  if (res.ok) return res.json();
  throw new Error('OCR failed');
}
