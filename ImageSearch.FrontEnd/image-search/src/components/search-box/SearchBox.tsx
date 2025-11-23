import { useState } from 'react';
import { HiSearch } from 'react-icons/hi';

interface SearchBoxProps {
    onSearch: (query: string) => void;
    isLoading: boolean;
}

const SearchBox = ({ onSearch, isLoading }: SearchBoxProps) => {
    const [query, setQuery] = useState('');
    const [error, setError] = useState<string>('');

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        const trimmedQuery = query.trim();

        if (trimmedQuery.length < 2) {
            setError('Please enter at least 2 characters');
            return;
        }

        setError('');
        onSearch(trimmedQuery);
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            handleSubmit(e);
        }
    };

    return (
        <div className="py-8">
            <div className="relative">
                <HiSearch className="absolute left-4 top-1/2 -translate-y-1/2 w-6 h-6 text-gray-400" />
                <input
                    className="w-full pl-12 pr-4 py-4 text-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary transition-shadow"
                    placeholder="Search for images..." type="text"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    disabled={isLoading}
                    aria-label="Search for images"
                    aria-describedby="search-hint"
                    autoComplete="off"
                    onKeyDown={handleKeyDown}
                />

                <p id="search-hint" className="sr-only">
                    Enter a keyword to search for images from Unsplash
                </p>
            </div>
            {error && <p className="text-red-500 text-sm mt-2">{error}</p>}
        </div>
    );
};

export default SearchBox;