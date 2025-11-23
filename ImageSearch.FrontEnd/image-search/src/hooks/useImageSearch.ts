import { useState, useRef, useCallback } from 'react';
import type { SearchResponse } from '../types/imageSearch.types';
import { searchImages } from '../services/api/imageSearchApi';
export const useImageSearch = () => {
    const [searchResult, setSearchResult] = useState<SearchResponse | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const abortControllerRef = useRef<AbortController | null>(null);

    const handleSearch = useCallback(async (query: string) => {
        // Cancel previous request if it exists
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
        }

        // Create new abort controller for this request
        abortControllerRef.current = new AbortController();

        setIsLoading(true);
        setError(null);

        try {
            const result = await searchImages(query, abortControllerRef.current.signal);
            setSearchResult(result);
        } catch (err) {
            // Don't show error if request was aborted
            if (err instanceof Error && err.name === 'AbortError') {
                return;
            }

            const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred';
            setError(errorMessage);
            setSearchResult(null);
        } finally {
            setIsLoading(false);
            abortControllerRef.current = null;
        }
    }, []);

    const handleDismissError = () => {
        setError(null);
    };

    return {
        searchResult,
        isLoading,
        error,
        handleSearch,
        handleDismissError
    };
}