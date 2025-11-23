import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import SearchBox from './SearchBox';

describe('SearchBox', () => {

  it('shows validation error when query is less than 2 characters', async () => {
    const user = userEvent.setup();
    const mockOnSearch = vi.fn();
    render(<SearchBox onSearch={mockOnSearch} isLoading={false} />);

    const input = screen.getByRole('textbox', { name: /search for images/i });
    await user.type(input, 'a');
    await user.keyboard('{Enter}');

    expect(screen.getByText('Please enter at least 2 characters')).toBeInTheDocument();
    expect(mockOnSearch).not.toHaveBeenCalled();
  });

  it('shows validation error when query is empty', async () => {
    const user = userEvent.setup();
    const mockOnSearch = vi.fn();
    render(<SearchBox onSearch={mockOnSearch} isLoading={false} />);

    const input = screen.getByRole('textbox', { name: /search for images/i });
    await user.click(input);
    await user.keyboard('{Enter}');

    expect(screen.getByText('Please enter at least 2 characters')).toBeInTheDocument();
    expect(mockOnSearch).not.toHaveBeenCalled();
  });

  it('calls onSearch with trimmed query when Enter is pressed', async () => {
    const user = userEvent.setup();
    const mockOnSearch = vi.fn();
    render(<SearchBox onSearch={mockOnSearch} isLoading={false} />);

    const input = screen.getByRole('textbox', { name: /search for images/i });
    await user.type(input, '  cars  ');
    await user.keyboard('{Enter}');

    expect(mockOnSearch).toHaveBeenCalledWith('cars');
    expect(mockOnSearch).toHaveBeenCalledTimes(1);
  });

  it('accepts queries with exactly 2 characters', async () => {
    const user = userEvent.setup();
    const mockOnSearch = vi.fn();
    render(<SearchBox onSearch={mockOnSearch} isLoading={false} />);

    const input = screen.getByRole('textbox', { name: /search for images/i });
    await user.type(input, 'ab');
    await user.keyboard('{Enter}');

    expect(mockOnSearch).toHaveBeenCalledWith('ab');
    expect(screen.queryByText('Please enter at least 2 characters')).not.toBeInTheDocument();
  });

  it('does not call onSearch when query is whitespace', async () => {
    const user = userEvent.setup();
    const mockOnSearch = vi.fn();
    render(<SearchBox onSearch={mockOnSearch} isLoading={false} />);

    const input = screen.getByRole('textbox', { name: /search for images/i });
    await user.type(input, '   ');
    await user.keyboard('{Enter}');

    expect(screen.getByText('Please enter at least 2 characters')).toBeInTheDocument();
    expect(mockOnSearch).not.toHaveBeenCalled();
  });

  it('clears error message when valid query is entered', async () => {
    const user = userEvent.setup();
    const mockOnSearch = vi.fn();
    render(<SearchBox onSearch={mockOnSearch} isLoading={false} />);

    const input = screen.getByRole('textbox', { name: /search for images/i });

    // First, trigger error
    await user.type(input, 'a');
    await user.keyboard('{Enter}');
    expect(screen.getByText('Please enter at least 2 characters')).toBeInTheDocument();

    // Enter valid query
    await user.clear(input);
    await user.type(input, 'cars');
    await user.keyboard('{Enter}');

    expect(screen.queryByText('Please enter at least 2 characters')).not.toBeInTheDocument();
    expect(mockOnSearch).toHaveBeenCalledWith('cars');
  });

  it('disables input when loading', () => {
    render(<SearchBox onSearch={vi.fn()} isLoading={true} />);

    const input = screen.getByRole('textbox', { name: /search for images/i });
    expect(input).toBeDisabled();
  });

  it('enables input when not loading', () => {
    render(<SearchBox onSearch={vi.fn()} isLoading={false} />);

    const input = screen.getByRole('textbox', { name: /search for images/i });
    expect(input).not.toBeDisabled();
  });
});
