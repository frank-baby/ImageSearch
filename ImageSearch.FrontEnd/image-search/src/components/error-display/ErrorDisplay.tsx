import { HiExclamationCircle, HiX } from 'react-icons/hi';

interface ErrorDisplayProps {
    error: string;
    onDismiss?: () => void;
}

const ErrorDisplay = ({ error, onDismiss }: ErrorDisplayProps) => {
    return (
        <div className="container mx-auto px-4 py-8">
            <div className="max-w-2xl mx-auto bg-red-50 border border-red-200 rounded-lg p-6">
                <div className="flex items-start justify-between">
                    <div className="flex items-start space-x-3">
                        <div className="shrink-0">
                            <HiExclamationCircle className="h-6 w-6 text-red-600" />
                        </div>
                        <div className="flex-1">
                            <p className="text-red-700 text-sm">
                                {error}
                            </p>
                        </div>
                    </div>
                    {onDismiss && (
                        <button
                            type="button"
                            onClick={onDismiss}
                            className="shrink-0 text-red-600 hover:text-red-800 transition-colors"
                            aria-label="Dismiss error"
                        >
                            <HiX className="h-5 w-5" />
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ErrorDisplay;
