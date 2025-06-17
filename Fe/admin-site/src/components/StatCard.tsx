import { motion } from "framer-motion";
import { IconType } from "react-icons";

interface StatCardProps {
  name: string;
  icon?: IconType;
  value: number | string;
  color: string;
}

const StatCard: React.FC<StatCardProps> = ({
  name,
  icon: Icon,
  value,
  color,
}) => {
  return (
    <motion.div
      className="bg-gray-900 bg-opacity-95 backdrop-blur-md overflow-hidden shadow-lg rounded-xl border border-gray-300"
      whileHover={{ y: -5, boxShadow: "0 25px 50px -12px rgba(0, 0, 0, 0.5)" }}
    >
      <div className="px-4 py-5 sm:p-6">
        <span className="flex items-center text-sm font-medium text-gray-400">
          {Icon && (
            <Icon
              size={20}
              style={{ color: color, minWidth: "20px" }}
              className="mr-2"
            />
          )}
          {name}
        </span>
        <p className="mt-1 text-3xl font-semibold text-gray-100">{value}</p>
      </div>
    </motion.div>
  );
};
export default StatCard;
