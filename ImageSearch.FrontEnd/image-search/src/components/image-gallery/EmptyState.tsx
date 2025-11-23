interface EmptyStateProps {
    query: string;
}

const EmptyState = ({ query }: EmptyStateProps) => {
    return (
        <div className="container mx-auto px-4 py-8">
            <div className="text-center text-gray-600">
                <p>No images were successfully processed for "{query}"</p>
            </div>
        </div>
    );
};

export default EmptyState;
