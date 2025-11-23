import type { SearchResponse } from '../../types/imageSearch.types';
const API_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const getErrorMessage = async (response: Response): Promise<string> => {
    try {
        const contentType = response.headers.get('content-type');
        if (contentType?.includes('application/json')) {
            const data = await response.json();
            
            if (data.error) return data.error;
            if (data.message) return data.message;
            if (data.detail) return data.detail;
        }
    } catch {
        // If JSON parsing fails, fall through to default message
    }
    return response.statusText || `Request failed with status ${response.status}`;
};

/**
 * Converts relative image URLs to absolute URLs
 */
const normalizeImageUrl = (relativeUrl: string | null): string | null => {
    if (!relativeUrl) return null;
    // If URL is already absolute, return as-is
    if (relativeUrl.startsWith('http')) return relativeUrl;
    // Otherwise, prepend base URL
    return `${API_URL}${relativeUrl}`;
};

export const searchImages = async (query: string, signal?: AbortSignal): Promise<SearchResponse> => {
    try {
        const response = await fetch(`${API_URL}/images/search`, {
            method: 'POST',
            body: JSON.stringify({ searchQuery: query }),
            headers: {
                'Content-Type': 'application/json',
            },
            signal,
        });

        if (!response.ok) {
            const errorMessage = await getErrorMessage(response);
            throw new Error(errorMessage);
        }

        const data: SearchResponse = await response.json();

        // Normalize all image URLs to absolute URLs
        const normalizedData: SearchResponse = {
            ...data,
            processedImages: data.processedImages.map(image => ({
                ...image,
                thumbnailUrl: normalizeImageUrl(image.thumbnailUrl),
                smallImageUrl: normalizeImageUrl(image.smallImageUrl),
            })),
        };

        return normalizedData;
    } catch (error) {
        // Provide user-friendly message for network failures
        if (error instanceof TypeError && error.message === 'Failed to fetch') {
            throw new Error('Unable to connect to the server. Please check your internet connection or try again later.');
        }

        throw error;
    }
};