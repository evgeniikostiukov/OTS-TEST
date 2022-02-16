#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OTS_TEST.Data;
using OTS_TEST.Models;
using System.IO.Compression;
using System.Net;
using System.Xml;

namespace OTS_TEST.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderContext _context;

        public OrdersController(OrderContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string inn, string kpp)
        {
            if (!string.IsNullOrEmpty(inn) && !string.IsNullOrEmpty(kpp))
            {
                return View(await _context.Order.Where(x => x.INN == inn && x.KPP == kpp).ToListAsync());
            }
            return View(await _context.Order.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Name,IsElectronic,Competitive,FullName,INN,KPP")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Code,Name,IsElectronic,Competitive,FullName,INN,KPP")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var order = await _context.Order.FindAsync(id);
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetArchive()
        {
            await GetFtp();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(long id)
        {
            return _context.Order.Any(e => e.Id == id);
        }

        private async Task GetFtp()
        {
            const string FTPUrl = "ftp://ftp.zakupki.gov.ru/out/nsi/nsiPurchaseMethod/daily";
            var reqInfo = WebRequest.Create($"{FTPUrl}");
            reqInfo.Credentials = new NetworkCredential("fz223free", "fz223free");
            reqInfo.Method = WebRequestMethods.Ftp.ListDirectory;
            var respInfo = reqInfo.GetResponse();
            var streamInfo = new StreamReader(respInfo.GetResponseStream());

            var directories = new List<string>();

            string line = streamInfo.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                directories.Add(line);
                line = streamInfo.ReadLine();
            }

            streamInfo.Close();
            respInfo.Close();

            var request = WebRequest.Create($"{FTPUrl}/{directories.First()}");

            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential("fz223free", "fz223free");

            var response = request.GetResponse();

            var stream = response.GetResponseStream();

            var fs = new FileStream($"C:/{directories.First()}", FileMode.Create);

            var buffer = new byte[64];
            int size;

            while ((size = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, size);
            }

            fs.Close();
            response.Close();

            ZipFile.ExtractToDirectory($"C:/{directories.First()}", "C:/EXTRACT_FILES", true);

            var xDoc = new XmlDocument();
            xDoc.Load($"C:/EXTRACT_FILES/{Path.GetFileName(Directory.GetFiles("C:/EXTRACT_FILES").First())}");

            var xRoot = xDoc.DocumentElement;
            var orders = new List<Order>();
            if (xRoot != null)
            {
                var body = xRoot.LastChild;
                foreach (XmlNode item in body.ChildNodes)
                {
                    var methodData = item.LastChild;

                    var code = "";
                    var name = "";
                    var isElectronic = "";
                    var competitive = "";

                    var fullName = "";
                    var inn = "";
                    var kpp = "";

                    foreach (XmlNode mNode in methodData.ChildNodes)
                    {
                        if (mNode.LocalName == $"code")
                        {
                            code = mNode.LastChild.Value;
                        }
                        if (mNode.LocalName == "name")
                        {
                            name = mNode.LastChild.Value;
                        }
                        if (mNode.LocalName == "isElectronic")
                        {
                            isElectronic = mNode.LastChild.Value;
                        }
                        if (mNode.LocalName == "competitive")
                        {
                            competitive = mNode.LastChild.Value;
                        }
                        if (mNode.LocalName == "creator")
                        {
                            foreach (XmlNode cNode in mNode.ChildNodes)
                            {
                                if (cNode.LocalName == "fullName")
                                {
                                    fullName = cNode.LastChild.Value;
                                }
                                if (cNode.LocalName == "inn")
                                {
                                    inn = cNode.LastChild.Value;
                                }
                                if (cNode.LocalName == "kpp")
                                {
                                    kpp = cNode.LastChild.Value;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(code)
                           && !string.IsNullOrEmpty(name)
                           && !string.IsNullOrEmpty(isElectronic)
                           && !string.IsNullOrEmpty(competitive)
                           && !string.IsNullOrEmpty(fullName)
                           && !string.IsNullOrEmpty(inn)
                           && !string.IsNullOrEmpty(kpp))
                    {
                        orders.Add(new Order
                        {
                            Code = code,
                            Competitive = competitive == "true",
                            FullName = fullName,
                            INN = inn,
                            IsElectronic = isElectronic == "true",
                            KPP = kpp,
                            Name = name
                        });
                    }
                }

                if (orders.Any())
                {
                    await _context.Order.AddRangeAsync(orders);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}