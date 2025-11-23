import type { ProcessedImage } from '../../types/imageSearch.types';

interface ProcessingWarningsProps {
    failedImages: ProcessedImage[];
}

const ProcessingWarnings = ({ failedImages }: ProcessingWarningsProps) => {
    if (failedImages.length === 0) return null;

    return (
        <div className="mt-6 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
            <h3 className="text-yellow-800 font-semibold mb-2">Processing Warnings</h3>
            <ul className="text-yellow-700 text-sm space-y-1">
                {failedImages.map((img) => (
                    <li key={img.imageId} className="line-clamp-1">
                        Image {img.imageId}: {img.errorMessage}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default ProcessingWarnings;
