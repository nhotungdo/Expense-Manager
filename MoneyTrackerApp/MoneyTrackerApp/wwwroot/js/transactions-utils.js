/**
 * Transaction Utilities
 * Shared logic for validating, building DTOs, and API calls for transactions.
 */

export function validateTransactionInput(data) {
    const errors = [];
    if (!data.amount || data.amount <= 0) {
        errors.push("Số tiền phải lớn hơn 0");
    }
    if (!data.accountId) {
        errors.push("Vui lòng chọn ví");
    }
    if (!data.date) {
        errors.push("Vui lòng chọn ngày giao dịch");
    }
    // Category is technically optional in backend but usually required in UI
    if (!data.categoryId && !data.isTransfer) {
        // Warning or error? QuickAdd enforces it. Let's enforce it for consistency.
        // errors.push("Vui lòng chọn danh mục");
    }

    return {
        valid: errors.length === 0,
        errors
    };
}

export function buildTransactionDto(raw) {
    return {
        TransactionType: raw.type, // 1=Income, 2=Expense
        Amount: raw.amount,
        AccountId: parseInt(raw.accountId),
        CategoryId: raw.categoryId ? parseInt(raw.categoryId) : null,
        TransactionDate: raw.date, // ISO string or yyyy-MM-dd
        Note: raw.note,
        Currency: raw.currency || 'VND',
        IsRecurring: false // Default
    };
}

export async function createTransaction(dto) {
    try {
        const response = await fetch('/api/Transactions', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dto),
            credentials: 'include' // Important for cookie auth
        });

        if (response.ok) {
            const data = await response.json();
            return { ok: true, data };
        } else {
            let errorMsg = 'Lỗi không xác định';
            try {
                const err = await response.json();
                errorMsg = err.message || JSON.stringify(err);
            } catch (e) {
                errorMsg = await response.text();
            }
            return { ok: false, error: errorMsg };
        }
    } catch (error) {
        return { ok: false, error: error.message };
    }
}
