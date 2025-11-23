import { HiViewGrid } from "react-icons/hi";

const Header = () => {
    return (
        <header className="py-6 flex flex-col md:flex-row justify-between items-center gap-4 border-b border-gray-200">
            <div className="flex items-center gap-3">
                <HiViewGrid className="w-8 h-8 text-primary" />
                <span className="text-xl font-bold text-gray-900">
                    Unsplash Gallery
                </span>
            </div>
        </header>
    );
};

export default Header;
