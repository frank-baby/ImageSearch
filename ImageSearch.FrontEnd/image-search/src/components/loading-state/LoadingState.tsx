const LoadingState = () => {
    return (
        <div className="container mx-auto px-4 py-12">
            <div className="flex flex-col items-center justify-center">
                <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-gray-900 dark:border-white"></div>
                <p className="text-gray-600 text-lg">Searching for images...</p>
                <p className="text-gray-500 text-sm mt-2">This may take a few moments</p>
            </div>
        </div>
    );
};

export default LoadingState;
