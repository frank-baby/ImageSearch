import { HiSearch } from 'react-icons/hi';
import Header from './components/header/Header';
import SearchBox from './components/search-box/SearchBox';
import ImageGallery from './components/image-gallery/ImageGallery';
import LoadingState from './components/loading-state/LoadingState';
import ErrorDisplay from './components/error-display/ErrorDisplay';
import { useImageSearch } from './hooks/useImageSearch';

function App() {
  const { searchResult, isLoading, error, handleSearch, handleDismissError } = useImageSearch();

  return (
    <div className="bg-background-light dark:bg-background-dark font-display text-gray-800 dark:text-gray-200 antialiased min-h-screen max-w-screen-2xl mx-auto">
      <Header />
      <SearchBox onSearch={handleSearch} isLoading={isLoading} />

      {error && <ErrorDisplay error={error} onDismiss={handleDismissError} />}

      {isLoading && <LoadingState />}

      {!isLoading && !error && searchResult && (
        <ImageGallery searchResult={searchResult} />
      )}

      {!isLoading && !error && !searchResult && (
        <div className="container mx-auto px-4 py-12">
          <div className="text-center text-gray-500">
            <HiSearch className="mx-auto h-16 w-16 text-gray-400 mb-4" />
            <p className="text-xl">Search for images to get started</p>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
